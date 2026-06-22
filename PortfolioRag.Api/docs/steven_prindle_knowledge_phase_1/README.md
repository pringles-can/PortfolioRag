# Steven Prindle Knowledge Base — Phase 1

This knowledge base was created from Steven Prindle's resume and is structured for RAG ingestion with pgvector.

## Purpose

The goal is to make resume and career information easier to retrieve semantically. Instead of embedding one large resume document, the content is split into smaller Markdown files organized by topic, project, technology, accomplishment, and interview-style answers.

## Suggested Ingestion Strategy

- Treat each Markdown file as a source document.
- Preserve the file path as metadata.
- Preserve headings as chunk metadata where possible.
- Chunk by Markdown sections rather than arbitrary character count.
- Use overlap only when a section is long.
- Store metadata such as:
  - source_path
  - document_type
  - company
  - project
  - technologies
  - topics

## Folder Structure

- `resume/` — summary, timeline, skills, education
- `companies/` — company-level experience summaries
- `projects/` — project-specific knowledge documents
- `accomplishments/` — measurable career outcomes
- `technologies/` — technology-specific experience
- `interview/` — reusable interview-style knowledge documents
