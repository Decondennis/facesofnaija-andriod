import React, { useCallback, useEffect, useState } from 'react';
import {
  FlatList,
  RefreshControl,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import EmptyState from '../../components/EmptyState';
import LoadingSpinner from '../../components/LoadingSpinner';
import UserAvatar from '../../components/UserAvatar';
import { useAuth } from '../../context/AuthContext';
import { getNotifications, type Notification } from '../../services/notificationService';
import { BASE_URL } from '../../config/api';

function NotificationItem({ item }: { item: Notification }) {
  const avatarUri = item.notifier?.avatar
    ? item.notifier.avatar.startsWith('http')
      ? item.notifier.avatar
      : `${BASE_URL}/${item.notifier.avatar}`
    : undefined;

  return (
    <View style={[styles.item, item.is_seen === '0' && styles.unread]}>
      <UserAvatar uri={avatarUri} name={item.notifier?.name} size={44} />
      <View style={styles.itemContent}>
        <Text style={styles.itemText}>{item.text ?? item.type}</Text>
        <Text style={styles.itemTime}>{item.time_text ?? item.time}</Text>
      </View>
    </View>
  );
}

export default function NotificationsScreen() {
  const { userId, sessionId } = useAuth();
  const [notifications, setNotifications] = useState<Notification[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadNotifications = useCallback(async () => {
    if (!userId || !sessionId) return;
    try {
      const result = await getNotifications(userId, sessionId);
      if (result.api_status === '200' && result.notifications) {
        setNotifications(result.notifications);
      }
    } catch {
      // silently fail
    }
  }, [userId, sessionId]);

  useEffect(() => {
    loadNotifications().finally(() => setLoading(false));
  }, [loadNotifications]);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadNotifications();
    setRefreshing(false);
  };

  if (loading) return <LoadingSpinner />;

  return (
    <FlatList
      style={styles.container}
      data={notifications}
      keyExtractor={(item) => item.id}
      renderItem={({ item }) => <NotificationItem item={item} />}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
      ListEmptyComponent={
        <EmptyState message="No notifications" subMessage="You're all caught up!" />
      }
      contentContainerStyle={notifications.length === 0 ? styles.emptyList : undefined}
    />
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f7fb' },
  emptyList: { flexGrow: 1 },
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
  unread: { backgroundColor: '#e8f0fe' },
  itemContent: { flex: 1, marginLeft: 12 },
  itemText: { fontSize: 14, color: '#333', lineHeight: 20 },
  itemTime: { fontSize: 12, color: '#999', marginTop: 4 },
});
