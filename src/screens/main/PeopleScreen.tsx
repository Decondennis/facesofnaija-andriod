import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert,
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import EmptyState from '../../components/EmptyState';
import LoadingSpinner from '../../components/LoadingSpinner';
import UserAvatar from '../../components/UserAvatar';
import { useAuth } from '../../context/AuthContext';
import {
  followUser,
  getUsersList,
  searchUsers,
  type UserProfile,
} from '../../services/userService';
import { BASE_URL } from '../../config/api';

function UserItem({
  user,
  onFollow,
}: {
  user: UserProfile;
  onFollow: (id: string) => void;
}) {
  const avatarUri = user.avatar
    ? user.avatar.startsWith('http')
      ? user.avatar
      : `${BASE_URL}/${user.avatar}`
    : undefined;

  return (
    <View style={styles.item}>
      <UserAvatar uri={avatarUri} name={user.name} size={48} />
      <View style={styles.itemContent}>
        <Text style={styles.name}>{user.name}</Text>
        <Text style={styles.username}>@{user.username}</Text>
      </View>
      <TouchableOpacity
        style={[styles.followButton, user.is_following === '1' && styles.followingButton]}
        onPress={() => onFollow(user.user_id)}
      >
        <Text
          style={[
            styles.followButtonText,
            user.is_following === '1' && styles.followingButtonText,
          ]}
        >
          {user.is_following === '1' ? 'Following' : 'Follow'}
        </Text>
      </TouchableOpacity>
    </View>
  );
}

export default function PeopleScreen() {
  const { userId, sessionId } = useAuth();
  const [users, setUsers] = useState<UserProfile[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [searching, setSearching] = useState(false);

  const loadUsers = useCallback(async () => {
    if (!userId || !sessionId) return;
    try {
      const result = await getUsersList(userId, sessionId, 30);
      if (result.api_status === '200' && result.users) {
        setUsers(result.users);
      }
    } catch {
      // silently fail
    }
  }, [userId, sessionId]);

  useEffect(() => {
    loadUsers().finally(() => setLoading(false));
  }, [loadUsers]);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadUsers();
    setRefreshing(false);
  };

  const handleSearch = async (text: string) => {
    setSearchQuery(text);
    if (!userId || !sessionId) return;
    if (text.trim().length < 2) {
      if (text.trim().length === 0) loadUsers();
      return;
    }
    setSearching(true);
    try {
      const result = await searchUsers(userId, sessionId, text.trim());
      if (result.api_status === '200' && result.users) {
        setUsers(result.users);
      }
    } catch {
      // silently fail
    } finally {
      setSearching(false);
    }
  };

  const handleFollow = async (followId: string) => {
    if (!userId || !sessionId) return;
    try {
      await followUser(userId, sessionId, followId);
      setUsers((prev) =>
        prev.map((u) =>
          u.user_id === followId
            ? { ...u, is_following: u.is_following === '1' ? '0' : '1' }
            : u
        )
      );
    } catch {
      Alert.alert('Error', 'Failed to follow user.');
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <View style={styles.container}>
      <TextInput
        style={styles.searchInput}
        placeholder="Search people..."
        placeholderTextColor="#aaa"
        value={searchQuery}
        onChangeText={handleSearch}
        autoCorrect={false}
      />
      <FlatList
        data={users}
        keyExtractor={(item) => item.user_id}
        renderItem={({ item }) => <UserItem user={item} onFollow={handleFollow} />}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        ListEmptyComponent={
          <EmptyState
            message={searching ? 'Searching...' : 'No people found'}
            subMessage={searching ? '' : 'Try a different search term'}
          />
        }
        contentContainerStyle={users.length === 0 ? styles.emptyList : undefined}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f7fb' },
  emptyList: { flexGrow: 1 },
  searchInput: {
    backgroundColor: '#fff',
    margin: 12,
    padding: 12,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: '#e0e0e0',
    fontSize: 15,
    color: '#222',
  },
  item: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: '#fff',
    padding: 14,
    marginHorizontal: 12,
    marginVertical: 4,
    borderRadius: 12,
    elevation: 1,
  },
  itemContent: { flex: 1, marginLeft: 12 },
  name: { fontWeight: '700', fontSize: 15, color: '#222' },
  username: { fontSize: 13, color: '#999', marginTop: 2 },
  followButton: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 8,
    backgroundColor: '#1a73e8',
  },
  followingButton: { backgroundColor: '#fff', borderWidth: 1, borderColor: '#1a73e8' },
  followButtonText: { color: '#fff', fontWeight: '600', fontSize: 13 },
  followingButtonText: { color: '#1a73e8' },
});
