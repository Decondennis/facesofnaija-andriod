import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert,
  Image,
  RefreshControl,
  ScrollView,
  StyleSheet,
  Text,
  TouchableOpacity,
  View,
} from 'react-native';
import LoadingSpinner from '../../components/LoadingSpinner';
import UserAvatar from '../../components/UserAvatar';
import { useAuth } from '../../context/AuthContext';
import { getUserData, type UserProfile } from '../../services/userService';
import { BASE_URL } from '../../config/api';

export default function ProfileScreen() {
  const { userId, sessionId, signOut } = useAuth();
  const [profile, setProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadProfile = useCallback(async () => {
    if (!userId || !sessionId) return;
    try {
      const result = await getUserData(userId, sessionId);
      if (result.api_status === '200' && result.userData) {
        setProfile(result.userData);
      }
    } catch {
      // silently fail
    }
  }, [userId, sessionId]);

  useEffect(() => {
    loadProfile().finally(() => setLoading(false));
  }, [loadProfile]);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadProfile();
    setRefreshing(false);
  };

  const handleLogout = () => {
    Alert.alert('Sign Out', 'Are you sure you want to sign out?', [
      { text: 'Cancel', style: 'cancel' },
      { text: 'Sign Out', style: 'destructive', onPress: signOut },
    ]);
  };

  if (loading) return <LoadingSpinner />;

  const avatarUri = profile?.avatar
    ? profile.avatar.startsWith('http')
      ? profile.avatar
      : `${BASE_URL}/${profile.avatar}`
    : undefined;

  const coverUri = profile?.cover
    ? profile.cover.startsWith('http')
      ? profile.cover
      : `${BASE_URL}/${profile.cover}`
    : undefined;

  return (
    <ScrollView
      style={styles.container}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
    >
      <View style={styles.coverContainer}>
        {coverUri ? (
          <Image source={{ uri: coverUri }} style={styles.cover} />
        ) : (
          <View style={[styles.cover, styles.coverPlaceholder]} />
        )}
        <View style={styles.avatarWrapper}>
          <UserAvatar uri={avatarUri} name={profile?.name} size={80} />
        </View>
      </View>

      <View style={styles.info}>
        <Text style={styles.name}>{profile?.name ?? 'Unknown User'}</Text>
        <Text style={styles.username}>@{profile?.username ?? ''}</Text>
        {profile?.about ? <Text style={styles.about}>{profile.about}</Text> : null}

        <View style={styles.stats}>
          <View style={styles.stat}>
            <Text style={styles.statNumber}>{profile?.posts_count ?? 0}</Text>
            <Text style={styles.statLabel}>Posts</Text>
          </View>
          <View style={styles.stat}>
            <Text style={styles.statNumber}>{profile?.followers_count ?? 0}</Text>
            <Text style={styles.statLabel}>Followers</Text>
          </View>
          <View style={styles.stat}>
            <Text style={styles.statNumber}>{profile?.following_count ?? 0}</Text>
            <Text style={styles.statLabel}>Following</Text>
          </View>
        </View>
      </View>

      <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
        <Text style={styles.logoutText}>Sign Out</Text>
      </TouchableOpacity>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f7fb' },
  coverContainer: { position: 'relative', marginBottom: 50 },
  cover: { width: '100%', height: 160 },
  coverPlaceholder: { backgroundColor: '#1a73e8' },
  avatarWrapper: {
    position: 'absolute',
    bottom: -40,
    left: 20,
    borderWidth: 3,
    borderColor: '#fff',
    borderRadius: 44,
  },
  info: { paddingHorizontal: 20, paddingTop: 8 },
  name: { fontSize: 22, fontWeight: '800', color: '#222' },
  username: { fontSize: 14, color: '#999', marginTop: 2 },
  about: { fontSize: 14, color: '#555', marginTop: 8, lineHeight: 20 },
  stats: { flexDirection: 'row', marginTop: 20, gap: 24 },
  stat: { alignItems: 'center' },
  statNumber: { fontSize: 20, fontWeight: '800', color: '#222' },
  statLabel: { fontSize: 12, color: '#999', marginTop: 2 },
  logoutButton: {
    margin: 20,
    padding: 14,
    backgroundColor: '#fff',
    borderRadius: 10,
    borderWidth: 1,
    borderColor: '#ff4444',
    alignItems: 'center',
  },
  logoutText: { color: '#ff4444', fontWeight: '700', fontSize: 15 },
});
