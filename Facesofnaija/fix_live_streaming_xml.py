from pathlib import Path
p = Path("Resources/layout/LiveStreamingLayout.xml")
t = p.read_text(encoding="utf-8")
old = """android:layout_weight="1"
                android:hint="@string/Lbl_Write_comment"
                <ImageView"""
new = """android:layout_weight="1"
                android:hint="@string/Lbl_Write_comment"
                android:imeOptions="actionSend"
                android:inputType="text"
                android:background="@null"
                        android:textColorHint="#444444"
                android:textSize="16sp"
                android:maxLines="4">
        </com.aghajari.emojiview.view.AXEmojiEditText>
                <ImageView"""
if old not in t:
    raise SystemExit("Old block not found")
p.write_text(t.replace(old, new), encoding="utf-8")
