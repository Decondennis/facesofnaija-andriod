import React from 'react';
import { Image, StyleSheet, Text, TouchableOpacity, View } from 'react-native';
import { BASE_URL } from '../config/api';
import type { Post } from '../services/postService';
import UserAvatar from './UserAvatar';

interface Props {
  post: Post;
  onPress?: () => void;
}

export default function PostCard({ post, onPress }: Props) {
  const avatarUri = post.publisher?.avatar
    ? post.publisher.avatar.startsWith('http')
      ? post.publisher.avatar
      : `${BASE_URL}/${post.publisher.avatar}`
    : undefined;

  const mediaUri = post.postFile
    ? post.postFile.startsWith('http')
      ? post.postFile
      : `${BASE_URL}/${post.postFile}`
    : undefined;

  return (
    <TouchableOpacity style={styles.card} onPress={onPress} activeOpacity={0.9}>
      <View style={styles.header}>
        <UserAvatar uri={avatarUri} name={post.publisher?.name} size={44} />
        <View style={styles.headerText}>
          <Text style={styles.name}>{post.publisher?.name ?? 'Unknown'}</Text>
          <Text style={styles.time}>{post.time_text ?? post.time}</Text>
        </View>
      </View>
      {post.Orginaltext ? (
        <Text style={styles.postText}>{post.Orginaltext}</Text>
      ) : null}
      {mediaUri ? (
        <Image source={{ uri: mediaUri }} style={styles.media} resizeMode="cover" />
      ) : null}
      <View style={styles.stats}>
        <Text style={styles.statText}>❤ {post.likes_count ?? 0}</Text>
        <Text style={styles.statText}>💬 {post.comment_count ?? 0}</Text>
      </View>
    </TouchableOpacity>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: '#fff',
    borderRadius: 12,
    marginHorizontal: 12,
    marginVertical: 6,
    padding: 14,
    shadowColor: '#000',
    shadowOpacity: 0.06,
    shadowRadius: 6,
    shadowOffset: { width: 0, height: 2 },
    elevation: 2,
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 10,
  },
  headerText: {
    marginLeft: 10,
  },
  name: {
    fontWeight: '700',
    fontSize: 15,
    color: '#222',
  },
  time: {
    fontSize: 12,
    color: '#999',
    marginTop: 2,
  },
  postText: {
    fontSize: 15,
    color: '#333',
    lineHeight: 22,
    marginBottom: 10,
  },
  media: {
    width: '100%',
    height: 200,
    borderRadius: 8,
    marginBottom: 10,
  },
  stats: {
    flexDirection: 'row',
    gap: 16,
    paddingTop: 8,
    borderTopWidth: 1,
    borderTopColor: '#f0f0f0',
  },
  statText: {
    fontSize: 13,
    color: '#666',
  },
});
