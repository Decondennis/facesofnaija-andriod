import zipfile
import struct
import os
import sys
import subprocess

def patch_aab(src_aab, dst_aab, keystore, alias, storepass, keypass):
    print(f"Reading: {src_aab}")
    patched_so = 0
    tmp_aab = dst_aab + ".tmp"
    with zipfile.ZipFile(src_aab, 'r') as zin, zipfile.ZipFile(tmp_aab, 'w', zipfile.ZIP_DEFLATED) as zout:
        for item in zin.infolist():
            if item.filename.startswith('META-INF/') and (item.filename.endswith('.SF') or item.filename.endswith('.RSA') or item.filename.endswith('.DSA') or item.filename.endswith('.MF')):
                continue
            
            data = bytearray(zin.read(item.filename))
            if item.filename.endswith('.so') and ('arm64' in item.filename or 'x86_64' in item.filename):
                if data[:4] == b'\x7fELF' and data[4] == 2:
                    e_phoff = struct.unpack_from('<Q', data, 0x20)[0]
                    e_phentsize = struct.unpack_from('<H', data, 0x36)[0]
                    e_phnum = struct.unpack_from('<H', data, 0x38)[0]
                    
                    modified = False
                    for i in range(e_phnum):
                        ph_offset = e_phoff + i * e_phentsize
                        p_type = struct.unpack_from('<I', data, ph_offset)[0]
                        if p_type == 1:
                            p_align = struct.unpack_from('<Q', data, ph_offset + 48)[0]
                            if p_align < 16384:
                                struct.pack_into('<Q', data, ph_offset + 48, 16384)
                                modified = True
                    if modified:
                        patched_so += 1
                        print(f"  [16KB Aligned] {item.filename}")
            zout.writestr(item, bytes(data))

    if os.path.exists(dst_aab):
        os.remove(dst_aab)
    os.rename(tmp_aab, dst_aab)
    print(f"Patched {patched_so} .so files to 16KB alignment.")

    jarsigner = r"C:\Program Files\Android\openjdk\jdk-21.0.8\bin\jarsigner.exe"
    cmd = [
        jarsigner, "-verbose", "-sigalg", "SHA256withRSA", "-digestalg", "SHA-256",
        "-keystore", keystore, "-storepass", storepass, "-keypass", keypass,
        dst_aab, alias
    ]
    res = subprocess.run(cmd, capture_output=True, text=True)
    if res.returncode != 0:
        print("Signing failed:", res.stderr)
        sys.exit(1)
    
    v_res = subprocess.run([jarsigner, "-verify", dst_aab], capture_output=True, text=True)
    print("Verification:", v_res.stdout.strip())

if __name__ == '__main__':
    src = sys.argv[1] if len(sys.argv) > 1 else 'Facesofnaija/bin/Release/net9.0-android36.0/com.facesofnaija.tlapp-Signed.aab'
    dst = sys.argv[2] if len(sys.argv) > 2 else 'PlayStore_Release/com.facesofnaija.tlapp-v2.1.2-Signed.aab'
    patch_aab(src, dst, 'Facesofnaija/Keystore/Facesofnaija.keystore', 'Facesofnaija', 'Chisom.22', 'Chisom.22')
