import { useEffect, useState, type FormEvent } from 'react'
import { BrowserRouter, NavLink, Navigate, Outlet, Route, Routes, useNavigate, useParams } from 'react-router-dom'
import {
  ArrowRight,
  ArrowUpRight,
  Eye,
  EyeOff,
  FileText,
  LayoutDashboard,
  Lock,
  LogOut,
  Plus,
  Search,
  SquarePen,
  Trash2,
} from 'lucide-react'
import { api, getAuthToken, logout, setAuthToken, type DashboardSummary, type Post, type PostStatus } from './api'
import './App.css'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<WorkspaceLayout />}>
            <Route index element={<Dashboard />} />
            <Route path="posts" element={<PostsPage />} />
            <Route path="posts/new" element={<PostEditor />} />
            <Route path="posts/:id/edit" element={<PostEditor />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

function ProtectedRoute() {
  const token = getAuthToken()
  return token ? <Outlet /> : <Navigate to="/login" replace />
}

function LoginPage() {
  const navigate = useNavigate()
  const [username, setUsername] = useState('admin')
  const [password, setPassword] = useState('Password123!')
  const [showPassword, setShowPassword] = useState(false)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (getAuthToken()) {
      navigate('/', { replace: true })
    }
  }, [navigate])

  async function handleLogin(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setSubmitting(true)

    try {
      const { data } = await api.post<{ token: string; username: string }>('/auth/login', { username, password })
      setAuthToken(data.token)
      navigate('/', { replace: true })
    } catch {
      setError('Invalid username or password. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="auth-shell">
      <div className="auth-card">
        <div className="auth-brand">
          <span className="brand-mark">f.</span>
          <span>fieldnotes</span>
        </div>
        <p className="auth-kicker">SIGN IN</p>
        <h1>Welcome back</h1>
        <p className="auth-subtitle">Use your content studio account to continue.</p>

        <form className="login-form" onSubmit={handleLogin}>
          {error && <p className="notice notice-error" role="alert">{error}</p>}
          <label className="field-label">
            Username
            <input className="text-input" value={username} onChange={(event) => setUsername(event.target.value)} autoComplete="username" required />
          </label>
          <div className="field-label">
            <label htmlFor="login-password">Password</label>
            <div className="password-input-wrap">
              <input id="login-password" className="text-input password-input" type={showPassword ? 'text' : 'password'} value={password} onChange={(event) => setPassword(event.target.value)} autoComplete="current-password" required />
              <button
                className="password-toggle"
                type="button"
                aria-label={showPassword ? 'Hide password' : 'Show password'}
                onClick={() => setShowPassword((visible) => !visible)}
              >
                {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
          </div>
          <button className="button button-primary" type="submit" disabled={submitting}>
            <Lock size={16} /> {submitting ? 'Signing in…' : 'Sign in'}
          </button>
        </form>

        <p className="demo-credentials">
          Demo access: <strong>admin</strong> / <strong>Password123!</strong>
        </p>
      </div>
    </div>
  )
}

function WorkspaceLayout() {
  return (
    <div className="workspace">
      <aside className="sidebar">
        <NavLink to="/" className="brand" aria-label="Fieldnotes home">
          <span className="brand-mark">f.</span>
          <span>fieldnotes<span className="brand-period">.</span></span>
        </NavLink>
        <p className="nav-caption">WORKSPACE</p>
        <nav className="main-nav" aria-label="Main navigation">
          <NavLink to="/" end className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
            <LayoutDashboard size={18} strokeWidth={1.8} /> Overview
          </NavLink>
          <NavLink to="/posts" className={({ isActive }) => `nav-link${isActive ? ' active' : ''}`}>
            <FileText size={18} strokeWidth={1.8} /> All posts
          </NavLink>
        </nav>
        <div className="sidebar-bottom">
          <span className="status-dot" />
          <span>Content studio</span>
          <button
            type="button"
            className="logout-button"
            onClick={() => {
              logout()
              window.location.href = '/login'
            }}
          >
            <LogOut size={14} />
            <span>Sign out</span>
          </button>
        </div>
      </aside>
      <main className="main-area">
        <header className="topbar">
          <span className="topbar-label">EDITORIAL / <span>CONTENT</span></span>
          <NavLink to="/posts/new" className="button button-primary topbar-create">
            <Plus size={17} /> New post
          </NavLink>
        </header>
        <div className="page-content"><Outlet /></div>
      </main>
    </div>
  )
}

function Dashboard() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get<DashboardSummary>('/posts/dashboard')
      .then(({ data }) => setSummary(data))
      .catch(() => setError('Could not load the dashboard. Check that the API is running.'))
  }, [])

  return (
    <>
      <div className="page-heading">
        <div>
          <p className="eyebrow">CONTENT OVERVIEW</p>
          <h1>Your writing desk</h1>
          <p className="heading-subtitle">A clear view of what you have been working on.</p>
        </div>
        <NavLink to="/posts/new" className="button button-dark"><Plus size={17} /> Write a post</NavLink>
      </div>

      {error && <p className="notice notice-error" role="alert">{error}</p>}
      <section className="stats-grid" aria-label="Post statistics">
        <Stat label="Total posts" value={summary?.totalPosts} icon={<FileText size={17} />} detail="Across your workspace" />
        <Stat label="Published" value={summary?.publishedPosts} icon={<ArrowUpRight size={17} />} detail="Live for your readers" tone="green" />
        <Stat label="Drafts" value={summary?.draftPosts} icon={<SquarePen size={17} />} detail="Still taking shape" tone="coral" />
      </section>

      <section className="section-block">
        <div className="section-heading">
          <div><p className="eyebrow">THE LATEST</p><h2>Recent posts</h2></div>
          <NavLink to="/posts" className="text-link">View all posts <ArrowRight size={16} /></NavLink>
        </div>
        <PostTable posts={summary?.recentPosts ?? []} loading={!summary && !error} emptyLabel="Nothing written yet. Your next post starts here." />
      </section>
      <div className="dashboard-note"><span className="note-mark">✳</span><p><strong>Good work takes a little room.</strong> Keep collecting ideas, and publish when they are ready.</p></div>
    </>
  )
}

function Stat({ label, value, icon, detail, tone = 'default' }: { label: string; value?: number; icon: React.ReactNode; detail: string; tone?: string }) {
  return (
    <div className={`stat-item stat-${tone}`}>
      <div className="stat-top"><span>{label}</span><span className="stat-icon">{icon}</span></div>
      <p className="stat-value">{value ?? '—'}</p>
      <p className="stat-detail">{detail}</p>
    </div>
  )
}

function PostsPage() {
  const [posts, setPosts] = useState<Post[]>([])
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [loadedQuery, setLoadedQuery] = useState('')
  const [error, setError] = useState('')
  const queryKey = `${search}\u0000${status}`

  useEffect(() => {
    const controller = new AbortController()
    api.get('/posts', { params: { page: 1, pageSize: 100, search: search || undefined, status: status === '' ? undefined : Number(status) }, signal: controller.signal })
      .then(({ data }) => {
        setPosts(data.items)
        setError('')
      })
      .catch(() => {
        if (controller.signal.aborted) return
        setError('Could not load posts. Check that the API is running.')
      })
      .finally(() => {
        if (!controller.signal.aborted) setLoadedQuery(queryKey)
      })
    return () => controller.abort()
  }, [search, status, queryKey])

  async function deletePost(post: Post) {
    if (!window.confirm(`Delete “${post.title}”? This cannot be undone.`)) return
    try {
      await api.delete(`/posts/${post.id}`)
      setPosts((current) => current.filter((item) => item.id !== post.id))
    } catch {
      setError('The post could not be deleted. Please try again.')
    }
  }

  return (
    <>
      <div className="page-heading">
        <div><p className="eyebrow">YOUR LIBRARY</p><h1>All posts</h1><p className="heading-subtitle">Keep every idea moving, from first draft to published.</p></div>
        <NavLink to="/posts/new" className="button button-dark"><Plus size={17} /> New post</NavLink>
      </div>
      <section className="section-block posts-section">
        <div className="list-toolbar">
          <label className="search-box"><Search size={17} /><input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search posts" aria-label="Search posts" /></label>
          <select className="filter-select" value={status} onChange={(event) => setStatus(event.target.value)} aria-label="Filter by status">
            <option value="">All statuses</option><option value="1">Published</option><option value="0">Draft</option>
          </select>
          <span className="result-count">{posts.length} {posts.length === 1 ? 'post' : 'posts'}</span>
        </div>
        {error && <p className="notice notice-error" role="alert">{error}</p>}
        <PostTable posts={posts} loading={loadedQuery !== queryKey} onDelete={deletePost} emptyLabel="No posts match this view. Try another search or write something new." />
      </section>
    </>
  )
}

function PostTable({ posts, loading, onDelete, emptyLabel }: { posts: Post[]; loading: boolean; onDelete?: (post: Post) => void; emptyLabel: string }) {
  if (loading) return <div className="table-message">Loading posts…</div>
  if (posts.length === 0) return <div className="table-message empty-message"><span className="empty-glyph">✳</span><p>{emptyLabel}</p></div>

  return (
    <div className="table-scroll">
      <table className="posts-table">
        <thead><tr><th>POST</th><th>STATUS</th><th>LAST UPDATED</th><th><span className="visually-hidden">Actions</span></th></tr></thead>
        <tbody>{posts.map((post) => (
          <tr key={post.id}>
            <td><div className="post-title-cell"><span className={`post-monogram ${post.status === 1 ? 'published-monogram' : ''}`}>{post.title.trim().charAt(0).toUpperCase() || 'N'}</span><div><NavLink to={`/posts/${post.id}/edit`} className="post-title">{post.title}</NavLink><p className="post-slug">/{post.slug}</p></div></div></td>
            <td><StatusBadge status={post.status} /></td>
            <td className="date-cell">{formatDate(post.updatedAt ?? post.createdAt)}</td>
            <td><div className="row-actions"><NavLink to={`/posts/${post.id}/edit`} className="icon-button" aria-label={`Edit ${post.title}`} title="Edit post"><SquarePen size={16} /></NavLink>{onDelete && <button type="button" className="icon-button danger-action" onClick={() => onDelete(post)} aria-label={`Delete ${post.title}`} title="Delete post"><Trash2 size={16} /></button>}</div></td>
          </tr>
        ))}</tbody>
      </table>
    </div>
  )
}

function StatusBadge({ status }: { status: PostStatus }) {
  return <span className={`status-badge ${status === 1 ? 'badge-published' : 'badge-draft'}`}><span />{status === 1 ? 'Published' : 'Draft'}</span>
}

function PostEditor() {
  const { id } = useParams()
  const navigate = useNavigate()
  const editing = Boolean(id)
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [excerpt, setExcerpt] = useState('')
  const [featuredImageUrl, setFeaturedImageUrl] = useState('')
  const [status, setStatus] = useState<PostStatus>(0)
  const [loading, setLoading] = useState(editing)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!id) return
    api.get<Post>(`/posts/${id}`)
      .then(({ data }) => {
        setTitle(data.title)
        setContent(data.content)
        setExcerpt(data.excerpt ?? '')
        setFeaturedImageUrl(data.featuredImageUrl ?? '')
        setStatus(data.status)
      })
      .catch(() => setError('This post could not be found.'))
      .finally(() => setLoading(false))
  }, [id])

  async function savePost(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSaving(true)
    setError('')
    const body = { title, content, excerpt: excerpt || null, featuredImageUrl: featuredImageUrl || null, status }
    try {
      if (editing) await api.put(`/posts/${id}`, body)
      else await api.post('/posts', body)
      navigate('/posts')
    } catch (requestError: unknown) {
      const message = requestError instanceof Error && 'response' in requestError
        ? ((requestError as { response?: { data?: { message?: string; title?: string } } }).response?.data?.message ?? 'Please check the form and try again.')
        : 'The post could not be saved. Check that the API is running.'
      setError(message)
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <div className="table-message">Loading post…</div>

  return (
    <>
      <div className="page-heading editor-heading">
        <div><p className="eyebrow">{editing ? 'YOUR LIBRARY / EDIT' : 'YOUR LIBRARY / NEW'}</p><h1>{editing ? 'Edit post' : 'A new story'}</h1><p className="heading-subtitle">Give the idea a title, then let it find its shape.</p></div>
      </div>
      <form className="editor-form" onSubmit={savePost}>
        {error && <p className="notice notice-error" role="alert">{error}</p>}
        <label className="field-label">Title<input className="text-input title-input" value={title} onChange={(event) => setTitle(event.target.value)} maxLength={200} required placeholder="Give your post a title" /></label>
        <div className="field-row">
          <label className="field-label">Excerpt <span className="optional">OPTIONAL</span><textarea className="text-input excerpt-input" value={excerpt} onChange={(event) => setExcerpt(event.target.value)} maxLength={500} rows={3} placeholder="A short introduction for your readers" /></label>
          <label className="field-label">Featured image URL <span className="optional">OPTIONAL</span><input className="text-input" type="url" value={featuredImageUrl} onChange={(event) => setFeaturedImageUrl(event.target.value)} placeholder="https://example.com/image.jpg" /></label>
        </div>
        <label className="field-label">Your story<textarea className="text-input content-input" value={content} onChange={(event) => setContent(event.target.value)} required rows={16} placeholder="Start with the part you cannot stop thinking about…" /></label>
        <div className="editor-footer">
          <label className="status-control">Status<select value={status} onChange={(event) => setStatus(Number(event.target.value) as PostStatus)}><option value={0}>Draft</option><option value={1}>Published</option></select></label>
          <div className="editor-actions"><button type="button" className="button button-quiet" onClick={() => navigate(-1)}>Cancel</button><button type="submit" className="button button-dark" disabled={saving}>{saving ? 'Saving…' : editing ? 'Save changes' : 'Create post'} <ArrowRight size={16} /></button></div>
        </div>
      </form>
    </>
  )
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric', year: 'numeric' }).format(new Date(value))
}

export default App