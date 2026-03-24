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
import { getMessages, type Message } from '../../services/messageService';
import { BASE_URL } from '../../config/api';

function MessageItem({ item }: { item: Message }) {
  const avatarUri = item.from?.avatar
    ? item.from.avatar.startsWith('http')
      ? item.from.avatar
      : `${BASE_URL}/${item.from.avatar}`
    : undefined;

  return (
    <View style={styles.item}>
      <UserAvatar uri={avatarUri} name={item.from?.name} size={48} />
      <View style={styles.itemContent}>
        <Text style={styles.senderName}>{item.from?.name ?? 'Unknown'}</Text>
        <Text style={styles.messageText} numberOfLines={1}>
          {item.text ?? '📎 Attachment'}
        </Text>
      </View>
      <Text style={styles.time}>{item.time_text ?? item.time}</Text>
    </View>
  );
}

export default function MessagesScreen() {
  const { userId, sessionId } = useAuth();
  const [messages, setMessages] = useState<Message[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const loadMessages = useCallback(async () => {
    if (!userId || !sessionId) return;
    try {
      const result = await getMessages(userId, sessionId);
      if (result.api_status === '200' && result.messages) {
        setMessages(result.messages);
      }
    } catch (err) {
      console.error('Failed to load messages:', err);
    }
  }, [userId, sessionId]);

  useEffect(() => {
    loadMessages().finally(() => setLoading(false));
  }, [loadMessages]);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadMessages();
    setRefreshing(false);
  };

  if (loading) return <LoadingSpinner />;

  return (
    <FlatList
      style={styles.container}
      data={messages}
      keyExtractor={(item) => item.id}
      renderItem={({ item }) => <MessageItem item={item} />}
      refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
      ListEmptyComponent={
        <EmptyState message="No messages yet" subMessage="Start a conversation!" />
      }
      contentContainerStyle={messages.length === 0 ? styles.emptyList : undefined}
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
  itemContent: { flex: 1, marginLeft: 12 },
  senderName: { fontWeight: '700', fontSize: 15, color: '#222' },
  messageText: { fontSize: 13, color: '#888', marginTop: 2 },
  time: { fontSize: 12, color: '#999' },
});
