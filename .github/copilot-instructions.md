# Copilot operations guidelines

This file contains instructions for Copilot on how to operate in this repository.

## General guidelines

- Use the latest version of the codebase as context for your suggestions.

## Pull Requests

- When writing pull requests, ensure that the title is descriptive and summarizes the changes made.

## Git Commit Messages

- Use the present tense for commit messages (e.g., "Add feature" instead of "Added feature").

## Available MCP Tools

### Filesystem Operations
- `mcp__filesystem__read_file` - Read file contents with optional head/tail
- `mcp__filesystem__read_multiple_files` - Read multiple files simultaneously
- `mcp__filesystem__write_file` - Create or overwrite files
- `mcp__filesystem__edit_file` - Make line-based edits to files
- `mcp__filesystem__create_directory` - Create directories
- `mcp__filesystem__list_directory` - List directory contents
- `mcp__filesystem__list_directory_with_sizes` - List with file sizes
- `mcp__filesystem__directory_tree` - Get recursive directory tree
- `mcp__filesystem__move_file` - Move or rename files
- `mcp__filesystem__search_files` - Search for files by pattern
- `mcp__filesystem__get_file_info` - Get file metadata
- `mcp__filesystem__list_allowed_directories` - Show accessible directories

### GitHub Integration
- `mcp__github__get_file_contents` - Read repository files
- `mcp__github__create_or_update_file` - Create/update repository files
- `mcp__github__push_files` - Push multiple files in single commit
- `mcp__github__create_repository` - Create new repositories
- `mcp__github__fork_repository` - Fork repositories
- `mcp__github__create_branch` - Create new branches
- `mcp__github__list_commits` - List repository commits
- `mcp__github__list_issues` - List repository issues
- `mcp__github__create_issue` - Create new issues
- `mcp__github__update_issue` - Update existing issues
- `mcp__github__add_issue_comment` - Add comments to issues
- `mcp__github__get_issue` - Get specific issue details
- `mcp__github__list_pull_requests` - List pull requests
- `mcp__github__create_pull_request` - Create new pull requests
- `mcp__github__get_pull_request` - Get PR details
- `mcp__github__get_pull_request_files` - Get PR file changes
- `mcp__github__get_pull_request_status` - Get PR status
- `mcp__github__get_pull_request_comments` - Get PR comments
- `mcp__github__get_pull_request_reviews` - Get PR reviews
- `mcp__github__create_pull_request_review` - Create PR reviews
- `mcp__github__merge_pull_request` - Merge pull requests
- `mcp__github__update_pull_request_branch` - Update PR branch
- `mcp__github__search_repositories` - Search repositories
- `mcp__github__search_code` - Search code
- `mcp__github__search_issues` - Search issues
- `mcp__github__search_users` - Search users

### SonarQube Integration
- `mcp__sonarqube__search_my_sonarqube_projects` - Find SonarQube projects
- `mcp__sonarqube__search_sonar_issues_in_projects` - Search issues in projects
- `mcp__sonarqube__change_sonar_issue_status` - Change issue status
- `mcp__sonarqube__get_project_quality_gate_status` - Get quality gate status
- `mcp__sonarqube__show_rule` - Show detailed rule information
- `mcp__sonarqube__list_rule_repositories` - List rule repositories
- `mcp__sonarqube__list_quality_gates` - List quality gates
- `mcp__sonarqube__list_languages` - List supported languages
- `mcp__sonarqube__analyze_code_snippet` - Analyze code snippets
- `mcp__sonarqube__get_component_measures` - Get component measures
- `mcp__sonarqube__search_metrics` - Search for metrics
- `mcp__sonarqube__get_scm_info` - Get SCM information
- `mcp__sonarqube__get_raw_source` - Get raw source code

### Container Management (Podman/Docker)
- `mcp__podman__container_list` - List containers
- `mcp__podman__container_run` - Run containers
- `mcp__podman__container_stop` - Stop containers
- `mcp__podman__container_remove` - Remove containers
- `mcp__podman__container_inspect` - Inspect container details
- `mcp__podman__container_logs` - View container logs
- `mcp__podman__image_list` - List images
- `mcp__podman__image_pull` - Pull images
- `mcp__podman__image_push` - Push images
- `mcp__podman__image_remove` - Remove images
- `mcp__podman__image_build` - Build images
- `mcp__podman__network_list` - List networks
- `mcp__podman__volume_list` - List volumes

### IDE Integration (JetBrains)
- `mcp__jetbrains__get_open_in_editor_file_text` - Get current file text
- `mcp__jetbrains__get_open_in_editor_file_path` - Get current file path
- `mcp__jetbrains__get_selected_in_editor_text` - Get selected text
- `mcp__jetbrains__replace_selected_text` - Replace selected text
- `mcp__jetbrains__replace_current_file_text` - Replace entire file
- `mcp__jetbrains__create_new_file_with_text` - Create new file
- `mcp__jetbrains__find_files_by_name_substring` - Find files by name
- `mcp__jetbrains__get_file_text_by_path` - Get file text by path
- `mcp__jetbrains__replace_file_text_by_path` - Replace file text
- `mcp__jetbrains__replace_specific_text` - Replace specific text
- `mcp__jetbrains__get_project_vcs_status` - Get VCS status
- `mcp__jetbrains__list_files_in_folder` - List folder contents
- `mcp__jetbrains__list_directory_tree_in_folder` - Get directory tree
- `mcp__jetbrains__search_in_files_content` - Search in file contents
- `mcp__jetbrains__run_configuration` - Run configurations
- `mcp__jetbrains__get_run_configurations` - Get available configurations
- `mcp__jetbrains__get_project_modules` - Get project modules
- `mcp__jetbrains__get_project_dependencies` - Get dependencies
- `mcp__jetbrains__toggle_debugger_breakpoint` - Toggle breakpoints
- `mcp__jetbrains__get_debugger_breakpoints` - Get breakpoints
- `mcp__jetbrains__open_file_in_editor` - Open files in editor
- `mcp__jetbrains__execute_action_by_id` - Execute IDE actions
- `mcp__jetbrains__get_current_file_errors` - Get file errors
- `mcp__jetbrains__reformat_current_file` - Reformat current file
- `mcp__jetbrains__get_project_problems` - Get project problems
- `mcp__jetbrains__execute_terminal_command` - Execute terminal commands

### Memory/Knowledge Management
- `mcp__memory__create_entities` - Create knowledge graph entities
- `mcp__memory__create_relations` - Create entity relations
- `mcp__memory__add_observations` - Add entity observations
- `mcp__memory__delete_entities` - Delete entities
- `mcp__memory__delete_observations` - Delete observations
- `mcp__memory__delete_relations` - Delete relations
- `mcp__memory__read_graph` - Read entire knowledge graph
- `mcp__memory__search_nodes` - Search graph nodes
- `mcp__memory__open_nodes` - Open specific nodes

### Advanced Tools
- `mcp__sequential-thinking__sequentialthinking` - Sequential thinking process
- `mcp__fetch__fetch` - Fetch URLs with content extraction
- `mcp__ide__getDiagnostics` - Get IDE diagnostics
