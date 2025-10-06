---
description: GitHub Spec Kit closeout workflow - handles staging, committing, PR creation, and branch cleanup
applyTo: '**'
---

# Closeout Workflow - GitHub Spec Kit

Automates the final steps of feature development: staging, committing, PR creation, and branch cleanup.

## 🎯 Workflow

1. **Check State** - Verify feature branch, view changes
2. **Stage Files** - Add source/docs/config, exclude artifacts
3. **Commit** - Use Conventional Commits format
4. **Push** - Sync to remote branch
5. **Create PR** - Use template with project-specific checks
6. **Switch to Main** - Return to baseline
7. **Next Steps** - Start new chat with `/specify`

---

## 📋 Phase 1: Repository State

```bash
git branch --show-current  # Must be feature branch
git status                  # View changes
git branch -r               # Check remotes
```

**Requirements:** On feature branch (not main/master), have uncommitted changes, determine target branch (main or master).

---

## 📦 Phase 2: File Staging

**Stage:** Source (`*.cs`, `*.js`, `*.jsx`, `*.css`, `*.md`, `*.json`), configs (`.github/**`, `*.config.js`), docs

**Exclude:** Build (`bin/`, `obj/`, `dist/`), dependencies (`node_modules/`, `.aspire/`), IDE (`.vs/`, `.idea/`), temp (`*.log`, `TestResults/`)

```bash
git add *.cs *.csproj *.js *.jsx *.css *.md *.json .github/
git diff --cached --name-status  # Verify
```

---

## 💬 Phase 3: Commit Creation

**Format:** `<type>(<scope>): <short summary>`

**Types:** `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`, `ci`, `build`

**Examples:**
```bash
git commit -m "feat(api): add boat speed multiplier endpoint"
git commit -m "fix(navigation): prevent waypoint oscillation at high speeds"
git commit -m "docs: add closeout workflow"
git commit -m "refactor(scene): migrate to atomic design"
```

---

## 🚀 Phase 4: Push

```bash
git push -u origin $(git branch --show-current)
```

**Errors:** Diverged branch → `git pull --rebase`, No upstream → use `-u`, Auth → `gh auth login`

---

## 📝 Phase 5: PR Creation

**Title:** `<type>(<scope>): <summary>`

**Template:**
```markdown
## 🎯 Objective
[One sentence: what this PR accomplishes]

## 🔍 Changes
- Added: [features]
- Modified: [updates]
- Fixed: [bugs]

## 🧪 Testing
- [ ] Manual: [scenarios tested]
- [ ] Automated: `dotnet test`, `npm run build`

## 📚 Documentation
- [ ] README/comments updated

## 🔗 Related Issues
Closes #[number]

## ✅ Project Checklist
- [ ] Aspire patterns (service discovery, `aspire run`)
- [ ] Atomic design (frontend hierarchy)
- [ ] Haversine formula, dynamic threshold (backend)
- [ ] CoordinateConverter, dock-centered coords (frontend)
- [ ] No hardcoded coordinates
- [ ] No console errors

## 🎓 Technical Context
**Decisions:** [key choices and rationale]
**Research:** [Microsoft Docs, Context7 sources]
**Breaking Changes:** [list or "None"]
```

**CLI:**
```bash
gh pr create --base main --head $(git branch --show-current) --web
```

**Best Practices:** Keep PRs focused, clear titles, include context, link issues, add screenshots for UI changes


---

## 🔄 Phase 6: Branch Cleanup

```bash
# Determine and switch to target branch
TARGET=$(git show-ref --verify --quiet refs/remotes/origin/main && echo "main" || echo "master")
git checkout "$TARGET"
git pull origin "$TARGET"
git status  # Verify clean
```

---

## 👤 Phase 7: Next Steps

**Closeout Complete!** Repository is clean and on main/master.

**To continue development:**
1. Start a new chat (clear context)
2. Use `/specify` command
3. Begin next feature

---

## 🛠️ Automation Script

```bash
#!/bin/bash
set -e
CURRENT=$(git branch --show-current)

# Validate not on main/master
[[ "$CURRENT" == "main" || "$CURRENT" == "master" ]] && 
  echo "❌ Cannot run on main/master" && exit 1

# Stage and commit
git add *.cs *.csproj *.js *.jsx *.css *.md *.json .github/ 2>/dev/null || true
[[ $(git diff --cached --name-only | wc -l) -eq 0 ]] && 
  echo "⚠️  No changes" && exit 0

echo "Commit message:"
read -r MSG
git commit -m "$MSG"

# Push
git push -u origin "$CURRENT"

# Create PR
TARGET=$(git show-ref --verify --quiet refs/remotes/origin/main && echo "main" || echo "master")
gh pr create --base "$TARGET" --head "$CURRENT" --web

# Return to main
read -p "Press Enter after PR created..."
git checkout "$TARGET" && git pull origin "$TARGET"
echo "✅ Done! Start new chat with /specify"
```

---

## 📚 Project Guidelines

**Backend (C# API):**
- Haversine formula for distance
- Dynamic threshold: `0.15 + (distanceTraveled * 1.5)`
- No hardcoded coordinates (use `Waypoint` records)
- Separation of concerns

**Frontend (React/Three.js):**
- Atomic design: atoms → molecules → organisms → templates
- `CoordinateConverter` for lat/lon ↔ scene
- Dock-centered origin: `(51.5100, -0.1350)`
- Status-based rendering

**Aspire:**
- Use `aspire run` only
- Service discovery via `WithReference()`
- OpenTelemetry configured

**Git Best Practices:**
- Atomic commits, descriptive messages
- Focused PRs (< 400 lines)
- Feature branches with clear names
- Delete branches after merge

---

## 🔍 Troubleshooting

| Issue | Solution |
|-------|----------|
| Nothing to commit | Changes in .gitignore or already committed |
| Permission denied | Run `gh auth login` |
| Branch diverged | `git pull --rebase origin <branch>` |
| PR exists | Push new commits to update existing PR |
| Can't find main | Check `git branch -r`, update TARGET variable |

---

## ✅ Verification

Before completing closeout:
- [ ] Relevant files staged (source, docs, config)
- [ ] Commit follows Conventional Commits
- [ ] Pushed to remote feature branch
- [ ] PR created with comprehensive description
- [ ] PR title clear and formatted correctly
- [ ] Related issues linked
- [ ] Testing/documentation updated
- [ ] No sensitive data committed
- [ ] Switched to main/master
- [ ] Working directory clean

---

## 🎓 Why This Workflow?

**Automated Staging:** Reduces errors, consistent patterns, respects .gitignore
**Structured Commits:** Searchable history, automated changelogs, easier debugging
**Comprehensive PRs:** Faster reviews, preserved context, documented decisions
**Clean Branches:** Prevents main commits, organized repo, parallel development

**GitHub Spec Kit Integration:** `/specify` → develop → **closeout** → review → merge → repeat

---

This workflow ensures consistent, high-quality PRs while automating tedious tasks and enforcing best practices across the development lifecycle.
