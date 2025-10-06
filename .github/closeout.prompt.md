---
description: GitHub Spec Kit closeout workflow - handles staging, committing, PR creation, and branch cleanup
applyTo: '**'
---

# Closeout Workflow - GitHub Spec Kit

This prompt orchestrates the final steps of a feature development cycle: staging changes, committing work, creating a pull request with modern best practices, and preparing for the next development iteration.

## 🎯 Workflow Overview

1. **Check Repository State** - Verify branch and uncommitted changes
2. **Review & Stage Changes** - Identify and stage appropriate files
3. **Commit Changes** - Create descriptive commit with context
4. **Push to Feature Branch** - Ensure remote is up-to-date
5. **Create Pull Request** - Generate PR with comprehensive description
6. **Switch to Main Branch** - Return to stable baseline
7. **Guide User** - Provide clear next steps

---

## 📋 Phase 1: Repository State Assessment

### Objective
Understand the current state of the repository before making any changes.

### Actions

```bash
# Check current branch
git branch --show-current

# View all changes (staged and unstaged)
git status --porcelain

# Show detailed status
git status

# Check remote branches
git branch -r

# Verify main/master branch exists
git show-ref --verify --quiet refs/remotes/origin/main || git show-ref --verify --quiet refs/remotes/origin/master
```

### Decision Points

- **Current Branch**: Must be on a feature branch, not main/master
- **Uncommitted Changes**: Identify files that need staging
- **Target Branch**: Determine if main or master is the default branch

---

## 📦 Phase 2: Intelligent File Staging

### Objective
Stage files that represent meaningful work while excluding build artifacts, dependencies, and temporary files.

### Files to ALWAYS Stage (if modified)

**Source Code:**
- `*.cs` - C# source files
- `*.csproj` - .NET project files
- `*.sln` - Solution files
- `*.js`, `*.jsx` - JavaScript/React files
- `*.ts`, `*.tsx` - TypeScript files
- `*.css` - Stylesheets
- `*.json` - Configuration files (package.json, appsettings.json, etc.)

**Documentation:**
- `*.md` - Markdown documentation
- `README.md`, `CHANGELOG.md`, `CONTRIBUTING.md`

**Configuration:**
- `.github/**` - GitHub workflows, templates, spec kit files
- `Dockerfile`, `docker-compose.yml`
- `vite.config.js`, `webpack.config.js`
- `.eslintrc.*`, `tsconfig.json`

### Files to NEVER Stage (unless explicitly required)

**Build Artifacts:**
- `bin/`, `obj/` - .NET build outputs
- `dist/`, `build/` - Frontend build outputs
- `*.dll`, `*.exe`, `*.so`

**Dependencies:**
- `node_modules/` - npm packages
- `packages/` - NuGet packages
- `.aspire/` - Aspire orchestration cache

**IDE/Editor:**
- `.vs/`, `.vscode/` (except settings.json if intentional)
- `.idea/` - JetBrains Rider
- `*.suo`, `*.user`

**Temporary:**
- `*.log`, `*.tmp`
- `.DS_Store`, `Thumbs.db`
- `TestResults/`, `coverage/`

### Staging Commands

```bash
# Review what will be staged
git diff --name-status

# Stage source code and documentation
git add *.cs *.csproj *.js *.jsx *.css *.md *.json

# Stage specific directories (if they contain source)
git add .github/ --ignore-errors

# Or use interactive staging for review
git add -p

# Verify what's staged
git diff --cached --name-status
```

### Validation

- Ensure `.gitignore` rules are respected
- Verify no sensitive data (API keys, credentials) is staged
- Confirm changes are related to the feature being developed

---

## 💬 Phase 3: Commit Creation

### Objective
Create a clear, descriptive commit that explains WHAT changed and WHY.

### Commit Message Format

Follow the **Conventional Commits** specification:

```
<type>(<scope>): <short summary>

<detailed description>

<optional footer>
```

### Commit Types

- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `style:` - Code style changes (formatting, no logic change)
- `refactor:` - Code refactoring (no feature change)
- `perf:` - Performance improvements
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks (dependencies, config)
- `ci:` - CI/CD pipeline changes
- `build:` - Build system changes

### Examples

```bash
# Feature addition
git commit -m "feat(api): add boat speed multiplier endpoint

Implements GET /api/boats?speed={1-10} to control simulation speed.
- Added speed query parameter validation
- Updated boat position calculation to use multiplier
- Added OpenAPI documentation for new parameter"

# Bug fix
git commit -m "fix(navigation): prevent waypoint oscillation at high speeds

Dynamic threshold now scales with distance traveled: 0.15 + (distance * 1.5).
This prevents boats from toggling between waypoints when using 10x speed."

# Documentation
git commit -m "docs: add closeout workflow for GitHub Spec Kit

Created .github/closeout.prompt.md to automate final PR workflow.
Includes staging logic, PR best practices, and branch switching."

# Refactoring
git commit -m "refactor(scene): migrate to atomic design pattern

Reorganized Three.js components into atoms/molecules/organisms.
- Moved Hull.js and Deck.js to atoms/
- Created DeckEquipment.js molecule
- Updated BoatModel.js to use new structure"
```

### Commit Command

```bash
# Single-line commit (for simple changes)
git commit -m "type(scope): short description"

# Multi-line commit (for complex changes)
git commit -m "type(scope): short description" -m "Detailed explanation of what changed and why. Include context that will help future developers understand the decision."

# Interactive commit (opens editor)
git commit
```

---

## 🚀 Phase 4: Push to Feature Branch

### Objective
Ensure the feature branch on remote matches local changes.

### Commands

```bash
# Get current branch name
BRANCH=$(git branch --show-current)

# Push to remote (create if doesn't exist)
git push -u origin "$BRANCH"

# If branch already exists remotely
git push origin "$BRANCH"

# Verify push succeeded
git log origin/"$BRANCH" -1
```

### Error Handling

- **Rejected push**: Branch has diverged, may need `git pull --rebase origin $BRANCH`
- **No upstream**: Use `-u` flag to set upstream tracking
- **Permission denied**: Check GitHub authentication

---

## 📝 Phase 5: Pull Request Creation

### Objective
Create a comprehensive, informative PR that facilitates efficient code review.

### PR Title Format

```
<type>(<scope>): <clear, concise summary>
```

**Examples:**
- `feat(api): add boat speed multiplier endpoint`
- `fix(navigation): prevent waypoint oscillation`
- `docs: add GitHub Spec Kit closeout workflow`
- `refactor(scene): migrate to atomic design pattern`

### PR Description Template

```markdown
## 🎯 Objective

[One-sentence summary of what this PR accomplishes]

## 🔍 Changes

### Added
- [ ] New feature X
- [ ] API endpoint Y
- [ ] Documentation Z

### Modified
- [ ] Updated component A to support B
- [ ] Refactored module C for better D

### Fixed
- [ ] Resolved issue #123
- [ ] Corrected bug in E

### Removed
- [ ] Deprecated function F
- [ ] Unused file G

## 🧪 Testing

### Manual Testing
- [ ] Tested scenario X with Y configuration
- [ ] Verified Z functionality works as expected
- [ ] Checked edge case A

### Automated Testing
- [ ] Unit tests pass: `dotnet test`
- [ ] Frontend builds: `npm run build`
- [ ] No linting errors: `npm run lint`

## 📚 Documentation

- [ ] Updated README.md
- [ ] Added code comments
- [ ] Updated API documentation
- [ ] Added/updated .github documentation

## 🔗 Related Issues

Closes #[issue-number]
Relates to #[issue-number]

## 📸 Screenshots (if applicable)

[Before/After images for UI changes]

## ✅ Checklist

- [ ] Code follows project conventions
- [ ] No console warnings or errors
- [ ] Respects .NET Aspire 9.5 patterns
- [ ] Follows atomic design principles (frontend)
- [ ] Backend uses Haversine formula for navigation
- [ ] No hardcoded coordinates
- [ ] Aspire orchestration tested with `aspire run`

## 🎓 Technical Context

**Architecture Decisions:**
- [Key decision 1 and rationale]
- [Key decision 2 and rationale]

**Research Sources:**
- Microsoft Docs: [links to official documentation used]
- Context7: [libraries referenced]

**Breaking Changes:**
- None / [List any breaking changes]

## 👥 Reviewers

@[suggested-reviewer-1]
@[suggested-reviewer-2]

---

**Note to Reviewers:**
Please focus on:
1. [Specific area of concern 1]
2. [Specific area of concern 2]
3. [Question or uncertainty that needs team input]
```

### GitHub CLI Command

```bash
# Get current branch and target branch
FEATURE_BRANCH=$(git branch --show-current)
TARGET_BRANCH="main"

# Check if master is default instead
if git show-ref --verify --quiet refs/remotes/origin/master; then
    TARGET_BRANCH="master"
fi

# Create PR with title and description
gh pr create \
  --base "$TARGET_BRANCH" \
  --head "$FEATURE_BRANCH" \
  --title "feat(scope): descriptive title" \
  --body "$(cat << 'EOF'
## 🎯 Objective

[Summary of changes]

## 🔍 Changes

- Added: [item]
- Modified: [item]
- Fixed: [item]

## 🧪 Testing

- [ ] Manual testing completed
- [ ] Automated tests pass

## ✅ Checklist

- [ ] Code follows conventions
- [ ] Documentation updated
- [ ] No breaking changes
EOF
)"

# Alternative: Open PR in browser for manual editing
gh pr create --web
```

### PR Best Practices

**DO:**
- ✅ Keep PRs focused (one feature/fix per PR)
- ✅ Write clear, descriptive titles
- ✅ Include context and rationale
- ✅ Add screenshots for UI changes
- ✅ Link related issues
- ✅ Use checklists for reviewers
- ✅ Request specific feedback
- ✅ Keep description up-to-date if scope changes

**DON'T:**
- ❌ Mix unrelated changes
- ❌ Use vague titles like "fix stuff" or "updates"
- ❌ Skip testing information
- ❌ Forget to update documentation
- ❌ Leave TODO comments in production code
- ❌ Include commented-out code
- ❌ Commit secrets or credentials

---

## 🔄 Phase 6: Branch Cleanup

### Objective
Return to the main branch, leaving the repository in a clean state for the next task.

### Commands

```bash
# Determine target branch
if git show-ref --verify --quiet refs/remotes/origin/main; then
    TARGET="main"
elif git show-ref --verify --quiet refs/remotes/origin/master; then
    TARGET="master"
else
    echo "Error: Neither main nor master branch found"
    exit 1
fi

# Switch to target branch
git checkout "$TARGET"

# Update local copy
git pull origin "$TARGET"

# Verify clean state
git status

# Optional: Delete feature branch locally (only if PR is merged)
# git branch -d feature-branch-name
```

### Verification

- Confirm you're on main/master: `git branch --show-current`
- Ensure no uncommitted changes: `git status`
- Verify up-to-date with remote: `git log origin/main..HEAD` (should be empty)

---

## 👤 Phase 7: User Guidance

### Objective
Provide clear next steps to the user for continuing development.

### Output Template

```markdown
## ✅ Closeout Complete

### Summary
- ✅ Changes staged: [X files]
- ✅ Commit created: [commit hash]
- ✅ Pushed to branch: [feature-branch-name]
- ✅ Pull Request: #[PR number] - [PR title]
- ✅ Switched to: [main/master]

### Pull Request Details

**URL:** [PR URL]

**Title:** [PR title]

**Status:** Open - awaiting review

**Reviewers:** [list if assigned]

### Next Steps

🎯 **Ready for your next task!**

To start working on a new feature:

1. **Start a new chat** (to clear context and get a fresh start)
2. **Use the /specify command** to define your next task
3. Copilot will guide you through the development process

---

**Alternative Commands:**

```bash
# Create a new feature branch
git checkout -b feature/your-new-feature

# Pull latest changes
git pull origin main

# View PR status
gh pr view [PR-number]

# View all your PRs
gh pr list --author "@me"
```

---

**Repository Status:**
- Branch: `main` (clean)
- Uncommitted changes: None
- Ready for new work: ✅

---

🚀 **Tip:** Use GitHub's review features to discuss specific lines of code with your team. Add inline comments, request changes, or approve the PR directly from the GitHub interface.
```

---

## 🛠️ Complete Workflow Script

### Bash Script for Full Automation

```bash
#!/bin/bash
set -e  # Exit on error

echo "🚀 Starting Closeout Workflow..."

# Phase 1: Check repository state
echo "📋 Phase 1: Checking repository state..."
CURRENT_BRANCH=$(git branch --show-current)
echo "Current branch: $CURRENT_BRANCH"

if [[ "$CURRENT_BRANCH" == "main" || "$CURRENT_BRANCH" == "master" ]]; then
    echo "❌ Error: Cannot run closeout on main/master branch"
    echo "Please create a feature branch first: git checkout -b feature/your-feature"
    exit 1
fi

# Phase 2: Stage changes
echo "📦 Phase 2: Staging changes..."
git status --porcelain

# Stage source files (adjust patterns as needed)
git add *.cs *.csproj *.js *.jsx *.css *.md *.json .github/ 2>/dev/null || true

STAGED_COUNT=$(git diff --cached --name-only | wc -l)
echo "Staged $STAGED_COUNT files"

if [ "$STAGED_COUNT" -eq 0 ]; then
    echo "⚠️  No files to commit. Working tree clean."
    exit 0
fi

git diff --cached --name-status

# Phase 3: Commit
echo "💬 Phase 3: Creating commit..."
echo "Enter commit message (type: short description):"
read -r COMMIT_MSG
git commit -m "$COMMIT_MSG"
COMMIT_HASH=$(git rev-parse --short HEAD)
echo "Created commit: $COMMIT_HASH"

# Phase 4: Push
echo "🚀 Phase 4: Pushing to remote..."
git push -u origin "$CURRENT_BRANCH"

# Phase 5: Create PR
echo "📝 Phase 5: Creating pull request..."

# Determine target branch
if git show-ref --verify --quiet refs/remotes/origin/main; then
    TARGET_BRANCH="main"
elif git show-ref --verify --quiet refs/remotes/origin/master; then
    TARGET_BRANCH="master"
else
    echo "❌ Error: Cannot determine target branch"
    exit 1
fi

echo "Target branch: $TARGET_BRANCH"

# Create PR (opens in browser for editing)
gh pr create --base "$TARGET_BRANCH" --head "$CURRENT_BRANCH" --web

echo "PR created! Complete the description in your browser."
echo "Press Enter when done..."
read -r

# Phase 6: Switch to main
echo "🔄 Phase 6: Switching to $TARGET_BRANCH..."
git checkout "$TARGET_BRANCH"
git pull origin "$TARGET_BRANCH"

# Phase 7: User guidance
echo ""
echo "✅ Closeout Complete!"
echo ""
echo "Summary:"
echo "  - Committed: $COMMIT_HASH"
echo "  - Branch: $CURRENT_BRANCH"
echo "  - Target: $TARGET_BRANCH"
echo ""
echo "🎯 Next Steps:"
echo "  1. Start a new chat"
echo "  2. Use /specify to define your next task"
echo ""
echo "Repository is clean and ready for new work! 🚀"
```

---

## 📚 Best Practices Reference

### Energy Boat Monitor Specific Guidelines

This project follows specific architectural patterns that should be verified before PR creation:

**Backend (C# API):**
- ✅ Uses Haversine formula for distance calculations
- ✅ Dynamic waypoint threshold: `0.15 + (distanceTraveled * 1.5)`
- ✅ No hardcoded coordinates (use `Waypoint` records)
- ✅ Proper separation of concerns (no frontend logic)

**Frontend (React/Three.js):**
- ✅ Follows atomic design: atoms → molecules → organisms → templates
- ✅ Uses `CoordinateConverter` for lat/lon ↔ scene conversion
- ✅ Dock-centered coordinate system: origin at `(51.5100, -0.1350)`
- ✅ Status-based rendering with proper material management

**Aspire Orchestration:**
- ✅ All commands through `aspire run` (no manual npm/dotnet commands)
- ✅ Service discovery via `WithReference()`
- ✅ OpenTelemetry configured for observability

### General Git/GitHub Best Practices

**Commits:**
- Atomic: One logical change per commit
- Descriptive: Clear message explaining what and why
- Tested: Each commit should leave the code in a working state

**PRs:**
- Focused: Single feature or fix per PR
- Documented: Clear description with context
- Tested: Include testing information and results
- Reviewable: Not too large (< 400 lines changed ideally)

**Branch Management:**
- Feature branches from main/master
- Descriptive names: `feature/add-speed-control`, `fix/waypoint-oscillation`
- Delete after merge to keep repository clean

---

## 🔍 Troubleshooting

### Common Issues

**"Nothing to commit"**
- All changes are already committed or in .gitignore
- Use `git status` to verify
- Check if changes are in unstaged files

**"Permission denied"**
- GitHub authentication not configured
- Run `gh auth login` to authenticate
- Verify SSH keys or personal access tokens

**"Branch has diverged"**
- Remote has commits not in local branch
- Run `git pull --rebase origin branch-name`
- Resolve conflicts if any

**"PR already exists"**
- A PR from this branch already exists
- Use `gh pr view` to see existing PR
- Update existing PR by pushing new commits

**"Cannot find main/master branch"**
- Repository uses different default branch
- Check with `git branch -r`
- Update TARGET_BRANCH variable accordingly

---

## 🎓 Educational Notes

### Why This Workflow?

**Automated Staging:**
- Reduces human error in file selection
- Ensures consistent patterns across team
- Respects .gitignore automatically

**Structured Commits:**
- Makes history searchable and understandable
- Enables automated changelog generation
- Facilitates debugging and rollbacks

**Comprehensive PRs:**
- Speeds up code review process
- Provides context for future reference
- Documents decisions and trade-offs

**Clean Branch Management:**
- Prevents accidental commits to main
- Keeps repository organized
- Enables parallel feature development

### Integration with GitHub Spec Kit

This closeout workflow is designed to work with GitHub Spec Kit's collaborative development model:

1. **Specify**: Define the task with `/specify`
2. **Develop**: Implement the feature
3. **Closeout**: Use this workflow to finalize and create PR
4. **Review**: Team reviews PR
5. **Merge**: Incorporate into main branch
6. **Repeat**: Start new chat with `/specify`

---

## ✅ Verification Checklist

Before considering closeout complete, verify:

- [ ] All relevant files are staged
- [ ] Commit message follows conventions
- [ ] Changes pushed to remote feature branch
- [ ] PR created with comprehensive description
- [ ] PR title is clear and follows format
- [ ] Related issues are linked
- [ ] Testing information is included
- [ ] Documentation is updated
- [ ] No sensitive data in commit
- [ ] Switched back to main/master branch
- [ ] Local main/master is up-to-date
- [ ] Working directory is clean

---

**End of Closeout Workflow**

This prompt provides a complete, automated workflow for finalizing feature development and creating high-quality pull requests. It ensures consistency, reduces errors, and maintains best practices across the development lifecycle.
