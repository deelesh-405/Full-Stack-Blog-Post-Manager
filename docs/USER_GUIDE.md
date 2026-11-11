# Fieldnotes User Guide

Fieldnotes is a private workspace for drafting and managing blog posts. This guide uses the sample content included with a fresh database.

## Sign in

Start the app using the Docker Compose or local instructions in the [README](../README.md), then open [http://localhost:5173](http://localhost:5173).

Use the local demo account:

- Username: `admin`
- Password: `Password123!`

![Fieldnotes sign-in screen](images/login.png)

These are development credentials, not suitable for a public deployment. Configure your own credentials and a strong JWT signing key before exposing the app.

## Overview

After signing in, the overview summarizes all posts, published posts, and drafts. The recent-posts table links directly to an item for editing. Choose **New post** or **Write a post** to start a story, or choose **All posts** in the sidebar to browse the library.

![Overview with sample post data](images/dashboard.png)

A fresh database is populated with four sample posts: three published posts and one draft. Sample posts are inserted only when the database contains no posts. Existing content is left untouched.

## Manage posts

The **All posts** page lists every post and its status. Search by text and filter by **Published** or **Draft**. Select a post title or its view control to read the full post, including its excerpt and featured image. From the detail page, choose **Edit post** to update it. The library's edit control opens the editor directly; its delete control asks for confirmation, and deletion cannot be undone.

![Post library with search and status filter](images/posts.png)

![Read-only post detail with featured image](images/post-detail.png)

To create a post, choose **New post** and enter a title and story. Excerpt and featured image URL are optional. Set the status to **Draft** to keep working privately or **Published** to publish it, then choose **Create post**. When editing an existing post, choose **Save changes** to apply updates.

## Sign out

Choose **Sign out** in the sidebar to end the current session. Protected pages and post API requests require signing in again after the token expires or is cleared.