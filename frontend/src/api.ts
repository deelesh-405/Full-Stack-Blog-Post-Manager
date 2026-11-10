import axios from 'axios'

export type PostStatus = 0 | 1

export interface Post {
  id: number
  title: string
  slug: string
  content: string
  excerpt: string | null
  featuredImageUrl: string | null
  status: PostStatus
  createdAt: string
  updatedAt: string | null
}

export interface DashboardSummary {
  totalPosts: number
  publishedPosts: number
  draftPosts: number
  recentPosts: Post[]
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5020/api',
  headers: { 'Content-Type': 'application/json' },
})