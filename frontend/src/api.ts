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

export function getAuthToken(): string | null {
  return localStorage.getItem('fieldnotes_token')
}

export function setAuthToken(token: string | null): void {
  if (token) {
    localStorage.setItem('fieldnotes_token', token)
    api.defaults.headers.common.Authorization = `Bearer ${token}`
    return
  }

  localStorage.removeItem('fieldnotes_token')
  delete api.defaults.headers.common.Authorization
}

export function logout(): void {
  setAuthToken(null)
}

api.interceptors.request.use((config) => {
  const token = getAuthToken()
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

const storedToken = getAuthToken()
if (storedToken) {
  setAuthToken(storedToken)
}