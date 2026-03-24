import React, { useCallback, useEffect, useState } from 'react';
import {
  Alert,
  FlatList,
  Modal,
  RefreshControl,
  StyleSheet,
  Text,
  TextInput,
  TouchableOpacity,
  View,
} from 'react-native';
import EmptyState from '../../components/EmptyState';
import LoadingSpinner from '../../components/LoadingSpinner';
import PostCard from '../../components/PostCard';
import { useAuth } from '../../context/AuthContext';
import { createPost, getFeedPosts, type Post } from '../../services/postService';

export default function HomeScreen() {
  const { userId, sessionId } = useAuth();
  const [posts, setPosts] = useState<Post[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [newPostText, setNewPostText] = useState('');
  const [showPostModal, setShowPostModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const loadPosts = useCallback(async () => {
    if (!userId || !sessionId) return;
    try {
      const result = await getFeedPosts(userId, sessionId, 20);
      if (result.api_status === '200' && result.posts) {
        setPosts(result.posts);
      }
    } catch (err) {
      console.error('Failed to load posts:', err);
    }
  }, [userId, sessionId]);

  useEffect(() => {
    loadPosts().finally(() => setLoading(false));
  }, [loadPosts]);

  const onRefresh = async () => {
    setRefreshing(true);
    await loadPosts();
    setRefreshing(false);
  };

  const handleCreatePost = async () => {
    if (!newPostText.trim()) {
      Alert.alert('Error', 'Please write something to post.');
      return;
    }
    if (!userId || !sessionId) return;
    setSubmitting(true);
    try {
      const result = await createPost(userId, sessionId, newPostText.trim());
      if (result.api_status === '200') {
        setNewPostText('');
        setShowPostModal(false);
        loadPosts();
      } else {
        Alert.alert('Error', result.errors?.error_text ?? 'Failed to create post.');
      }
    } catch {
      Alert.alert('Error', 'Could not connect to server.');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <LoadingSpinner />;

  return (
    <View style={styles.container}>
      <TouchableOpacity style={styles.newPostButton} onPress={() => setShowPostModal(true)}>
        <Text style={styles.newPostText}>What's on your mind?</Text>
      </TouchableOpacity>

      <FlatList
        data={posts}
        keyExtractor={(item) => item.id}
        renderItem={({ item }) => <PostCard post={item} />}
        refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        ListEmptyComponent={
          <EmptyState message="No posts yet" subMessage="Be the first to share something!" />
        }
        contentContainerStyle={posts.length === 0 ? styles.emptyList : undefined}
      />

      <Modal visible={showPostModal} animationType="slide" transparent>
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <Text style={styles.modalTitle}>New Post</Text>
            <TextInput
              style={styles.postInput}
              placeholder="What's on your mind?"
              placeholderTextColor="#aaa"
              multiline
              value={newPostText}
              onChangeText={setNewPostText}
              maxLength={5000}
            />
            <View style={styles.modalButtons}>
              <TouchableOpacity
                style={styles.cancelButton}
                onPress={() => setShowPostModal(false)}
              >
                <Text style={styles.cancelButtonText}>Cancel</Text>
              </TouchableOpacity>
              <TouchableOpacity
                style={[styles.postButton, submitting && styles.buttonDisabled]}
                onPress={handleCreatePost}
                disabled={submitting}
              >
                <Text style={styles.postButtonText}>{submitting ? 'Posting...' : 'Post'}</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#f5f7fb' },
  emptyList: { flexGrow: 1 },
  newPostButton: {
    backgroundColor: '#fff',
    margin: 12,
    padding: 14,
    borderRadius: 12,
    borderWidth: 1,
    borderColor: '#e0e0e0',
  },
  newPostText: { color: '#aaa', fontSize: 15 },
  modalOverlay: {
    flex: 1,
    backgroundColor: 'rgba(0,0,0,0.5)',
    justifyContent: 'flex-end',
  },
  modalContent: {
    backgroundColor: '#fff',
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
    padding: 20,
    paddingBottom: 40,
  },
  modalTitle: { fontSize: 18, fontWeight: '700', marginBottom: 14, color: '#222' },
  postInput: {
    borderWidth: 1,
    borderColor: '#e0e0e0',
    borderRadius: 10,
    padding: 12,
    fontSize: 15,
    minHeight: 100,
    textAlignVertical: 'top',
    color: '#222',
  },
  modalButtons: { flexDirection: 'row', gap: 12, marginTop: 14 },
  cancelButton: {
    flex: 1,
    padding: 14,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: '#e0e0e0',
    alignItems: 'center',
  },
  cancelButtonText: { color: '#555', fontWeight: '600', fontSize: 15 },
  postButton: {
    flex: 1,
    backgroundColor: '#1a73e8',
    padding: 14,
    borderRadius: 10,
    alignItems: 'center',
  },
  buttonDisabled: { opacity: 0.7 },
  postButtonText: { color: '#fff', fontWeight: '700', fontSize: 15 },
});
