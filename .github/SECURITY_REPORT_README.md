# GitHub Actions Security Report Setup

## Overview
This automated workflow generates beautiful HTML and Markdown security reports from your CodeQL scanning results.

## How It Works

### Workflow: `generate-security-report.yml`
- **Trigger:** Runs on schedule (daily at 9 AM UTC) or manually
- **Actions:**
  1. Fetches all CodeQL alerts from GitHub API
  2. Generates professional HTML report with charts and styling
  3. Generates readable Markdown report
  4. Commits both reports to your repository
  5. Exports SARIF format for other security tools

### Report Script: `generate_report.py`
- Fetches alerts using GitHub API
- Groups issues by severity (Critical/Warning/Note)
- Generates color-coded HTML report
- Creates summary statistics
- Includes direct links to GitHub alerts

## Generated Reports

### 1. `security-report.html`
- **Interactive**, beautiful HTML report
- Color-coded by severity
- Includes statistics dashboard
- Direct links to GitHub
- Can be opened in any browser
- Professional styling

### 2. `security-report.md`
- **Markdown format** for easy review in GitHub
- Summary table
- Organized by severity level
- Can be embedded in README
- Easy to share

## How to Use

### Manual Trigger
1. Go to Actions tab: https://github.com/Subharath/career-portal-slt/actions
2. Find "Generate Security Report" workflow
3. Click **Run workflow** → **Run workflow**

### Automatic (Scheduled)
- Runs daily at 9 AM UTC automatically
- Triggered after each code push
- Reports committed to repository

### View Reports

**In GitHub:**
```
root/
├── security-report.html  (View in browser)
├── security-report.md    (View in GitHub)
└── .github/
    ├── workflows/
    │   └── generate-security-report.yml
    └── scripts/
        └── generate_report.py
```

**Locally:**
```powershell
# Pull latest changes
git pull

# Open HTML report in browser
start security-report.html

# View Markdown report
Get-Content security-report.md

# View reports in VS Code
code security-report.html
code security-report.md
```

## Customization

### Change Schedule
Edit `.github/workflows/generate-security-report.yml`:
```yaml
schedule:
  - cron: '0 9 * * *'  # Change time (UTC)
```

Cron format: `minute hour day month weekday`
- `0 6 * * *` = 6 AM UTC daily
- `0 9 * * MON` = Monday 9 AM UTC
- `0 0 1 * *` = First of month at midnight UTC

### Include Dismissed Alerts
Edit `generate_report.py`:
```python
url = f'{self.api_base}/{self.repository}/code-scanning/alerts?state=open'
# Change to:
url = f'{self.api_base}/{self.repository}/code-scanning/alerts?state=open,dismissed'
```

## Troubleshooting

### "No changes to commit" message
- This is normal - it means reports haven't changed since last run
- Reports only update when new alerts are found

### SARIF export not available
- Not critical - workflow continues anyway
- SARIF is for advanced integrations

### Reports not appearing
1. Check Actions tab for workflow runs
2. Verify GitHub token permissions
3. Check branch protection rules (shouldn't affect this)
4. Ensure CodeQL is enabled in Security settings

## Next Steps

1. ✅ Push changes to GitHub:
   ```powershell
   git add .github/
   git commit -m "Add security report automation"
   git push
   ```

2. ✅ Trigger first run manually in Actions tab

3. ✅ View generated `security-report.html` and `security-report.md`

4. ✅ Share reports with team/stakeholders

## Security Notes

- GitHub token is automatically provided by Actions
- Token has limited scope (read security events, write contents)
- No credentials stored in repository
- Reports committed with bot signature
