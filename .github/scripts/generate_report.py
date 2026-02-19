#!/usr/bin/env python3
"""
GitHub CodeQL Security Report Generator
Fetches CodeQL alerts and generates HTML and Markdown reports
"""

import os
import requests
import json
from datetime import datetime
from typing import List, Dict

class CodeQLReportGenerator:
    def __init__(self):
        self.github_token = os.getenv('GITHUB_TOKEN')
        self.repository = os.getenv('GITHUB_REPOSITORY')
        self.headers = {
            'Authorization': f'token {self.github_token}',
            'Accept': 'application/vnd.github+json',
            'X-GitHub-Api-Version': '2022-11-28'
        }
        self.api_base = 'https://api.github.com/repos'
        self.alerts = []
        
    def fetch_alerts(self) -> List[Dict]:
        """Fetch all CodeQL scanning alerts from GitHub API"""
        try:
            url = f'{self.api_base}/{self.repository}/code-scanning/alerts?state=open'
            all_alerts = []
            
            while url:
                response = requests.get(url, headers=self.headers)
                response.raise_for_status()
                
                all_alerts.extend(response.json())
                
                # Handle pagination
                if 'Link' in response.headers:
                    links = response.headers['Link'].split(',')
                    url = None
                    for link in links:
                        if 'rel="next"' in link:
                            url = link.split(';')[0].strip('<>')
                else:
                    url = None
            
            self.alerts = all_alerts
            print(f"✓ Fetched {len(all_alerts)} CodeQL alerts")
            return all_alerts
            
        except Exception as e:
            print(f"✗ Error fetching alerts: {e}")
            return []
    
    def group_by_severity(self) -> Dict:
        """Group alerts by severity level"""
        grouped = {
            'error': [],
            'warning': [],
            'note': []
        }
        
        for alert in self.alerts:
            severity = alert.get('rule', {}).get('severity', 'note').lower()
            if severity in grouped:
                grouped[severity].append(alert)
        
        return grouped
    
    def generate_html_report(self, output_file: str = 'security-report.html'):
        """Generate HTML security report"""
        grouped = self.group_by_severity()
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        
        html_content = f"""<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>CodeQL Security Report</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: #333;
            padding: 20px;
        }}
        .container {{ 
            max-width: 1200px; 
            margin: 0 auto; 
            background: white; 
            border-radius: 10px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
            padding: 40px;
        }}
        header {{
            border-bottom: 3px solid #667eea;
            margin-bottom: 30px;
            padding-bottom: 20px;
        }}
        h1 {{ color: #333; font-size: 2.5em; margin-bottom: 10px; }}
        .timestamp {{ color: #666; font-size: 0.9em; }}
        .summary {{ 
            display: grid; 
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); 
            gap: 20px;
            margin-bottom: 40px;
        }}
        .summary-box {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
        }}
        .summary-box h3 {{ font-size: 2em; margin-bottom: 5px; }}
        .summary-box p {{ font-size: 0.9em; opacity: 0.9; }}
        .alert-section {{ margin-bottom: 40px; }}
        .alert-section h2 {{ 
            padding: 15px; 
            border-radius: 5px;
            margin-bottom: 20px;
            color: white;
            font-size: 1.5em;
        }}
        .severity-error {{ background-color: #dc3545; }}
        .severity-warning {{ background-color: #ffc107; color: #333; }}
        .severity-note {{ background-color: #17a2b8; }}
        .alert-item {{
            border-left: 4px solid #667eea;
            padding: 20px;
            margin-bottom: 15px;
            background: #f8f9fa;
            border-radius: 5px;
        }}
        .alert-title {{ 
            font-weight: bold; 
            font-size: 1.1em;
            color: #333;
            margin-bottom: 10px;
        }}
        .alert-details {{ 
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
            gap: 15px;
            font-size: 0.9em;
        }}
        .detail {{ 
            padding: 10px;
            background: white;
            border-radius: 3px;
        }}
        .detail-label {{ 
            font-weight: bold; 
            color: #667eea;
            margin-bottom: 5px;
        }}
        .rule-id {{ color: #666; font-family: monospace; }}
        .file-path {{ color: #007bff; font-family: monospace; word-break: break-all; }}
        .status {{ 
            display: inline-block;
            padding: 4px 8px;
            border-radius: 3px;
            font-size: 0.85em;
            font-weight: bold;
        }}
        .status-open {{ background: #f8d7da; color: #721c24; }}
        .status-dismissed {{ background: #d1ecf1; color: #0c5460; }}
        footer {{
            border-top: 1px solid #ddd;
            margin-top: 40px;
            padding-top: 20px;
            font-size: 0.9em;
            color: #666;
        }}
        .no-alerts {{ 
            text-align: center; 
            padding: 40px; 
            color: #666;
            background: #f0f0f0;
            border-radius: 5px;
        }}
        .no-alerts h3 {{ color: #28a745; font-size: 1.5em; }}
    </style>
</head>
<body>
    <div class="container">
        <header>
            <h1>🔒 CodeQL Security Report</h1>
            <p class="timestamp">Generated: {timestamp}</p>
            <p class="timestamp">Repository: {self.repository}</p>
        </header>
        
        <section class="summary">
            <div class="summary-box">
                <h3>{len(grouped['error'])}</h3>
                <p>Critical Issues</p>
            </div>
            <div class="summary-box">
                <h3>{len(grouped['warning'])}</h3>
                <p>Warnings</p>
            </div>
            <div class="summary-box">
                <h3>{len(grouped['note'])}</h3>
                <p>Notes</p>
            </div>
            <div class="summary-box">
                <h3>{len(self.alerts)}</h3>
                <p>Total Issues</p>
            </div>
        </section>
"""
        
        # Add alerts by severity
        severity_order = ['error', 'warning', 'note']
        severity_labels = {'error': '🔴 Critical', 'warning': '🟡 Warning', 'note': '🔵 Note'}
        
        for severity in severity_order:
            alerts = grouped[severity]
            if not alerts:
                continue
            
            severity_class = f'severity-{severity}'
            label = severity_labels[severity]
            
            html_content += f'<section class="alert-section">'
            html_content += f'<h2 class="{severity_class}">{label} ({len(alerts)})</h2>'
            
            for alert in sorted(alerts, key=lambda x: x.get('number', 0)):
                rule_name = alert.get('rule', {}).get('name', 'Unknown Rule')
                rule_id = alert.get('rule', {}).get('id', 'N/A')
                file_path = alert.get('most_recent_instance', {}).get('location', {}).get('path', 'N/A')
                line = alert.get('most_recent_instance', {}).get('location', {}).get('start_line', 'N/A')
                state = alert.get('state', 'open')
                url = alert.get('html_url', '#')
                
                status_class = f'status status-{state}'
                
                html_content += f"""
                <div class="alert-item">
                    <div class="alert-title"><a href="{url}" target="_blank" style="color: #667eea; text-decoration: none;">{rule_name}</a></div>
                    <div class="alert-details">
                        <div class="detail">
                            <div class="detail-label">Rule ID</div>
                            <div class="rule-id">{rule_id}</div>
                        </div>
                        <div class="detail">
                            <div class="detail-label">File</div>
                            <div class="file-path">{file_path}</div>
                        </div>
                        <div class="detail">
                            <div class="detail-label">Line</div>
                            <div>{line}</div>
                        </div>
                        <div class="detail">
                            <div class="detail-label">Status</div>
                            <div class="{status_class}">{state.upper()}</div>
                        </div>
                    </div>
                </div>
"""
            
            html_content += '</section>'
        
        # Handle case with no alerts
        if not self.alerts:
            html_content += """
            <div class="no-alerts">
                <h3>✓ No security issues detected!</h3>
                <p>Your code passed all CodeQL security checks.</p>
            </div>
"""
        
        html_content += """
        <footer>
            <p>This report was automatically generated by GitHub Actions CodeQL scanning.</p>
            <p>For more information, visit your <a href="https://github.com/""" + self.repository + """/security/code-scanning">Security alerts page</a>.</p>
        </footer>
    </div>
</body>
</html>
"""
        
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(html_content)
        
        print(f"✓ HTML report generated: {output_file}")
    
    def generate_markdown_report(self, output_file: str = 'security-report.md'):
        """Generate Markdown security report"""
        grouped = self.group_by_severity()
        timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        
        md_content = f"""# 🔒 CodeQL Security Report

**Generated:** {timestamp}  
**Repository:** {self.repository}

## Summary

| Severity | Count |
|----------|-------|
| 🔴 Critical | {len(grouped['error'])} |
| 🟡 Warning | {len(grouped['warning'])} |
| 🔵 Note | {len(grouped['note'])} |
| **TOTAL** | **{len(self.alerts)}** |

"""
        
        if not self.alerts:
            md_content += "## ✓ Status\n\n✅ No security issues detected! Your code passed all CodeQL security checks.\n"
        else:
            severity_order = ['error', 'warning', 'note']
            severity_labels = {'error': '🔴 Critical', 'warning': '🟡 Warning', 'note': '🔵 Note'}
            
            for severity in severity_order:
                alerts = grouped[severity]
                if not alerts:
                    continue
                
                label = severity_labels[severity]
                md_content += f"\n## {label} Issues ({len(alerts)})\n\n"
                
                for alert in sorted(alerts, key=lambda x: x.get('number', 0)):
                    rule_name = alert.get('rule', {}).get('name', 'Unknown Rule')
                    rule_id = alert.get('rule', {}).get('id', 'N/A')
                    file_path = alert.get('most_recent_instance', {}).get('location', {}).get('path', 'N/A')
                    line = alert.get('most_recent_instance', {}).get('location', {}).get('start_line', 'N/A')
                    state = alert.get('state', 'open')
                    url = alert.get('html_url', '#')
                    
                    md_content += f"""
### {rule_name}
- **Rule ID:** `{rule_id}`
- **File:** `{file_path}` (Line {line})
- **Status:** `{state.upper()}`
- **Link:** [{rule_id}]({url})

"""
        
        md_content += """---
*This report was automatically generated by GitHub Actions CodeQL scanning.*
"""
        
        with open(output_file, 'w', encoding='utf-8') as f:
            f.write(md_content)
        
        print(f"✓ Markdown report generated: {output_file}")

def main():
    """Main execution"""
    print("🚀 Starting CodeQL Report Generation...\n")
    
    generator = CodeQLReportGenerator()
    
    # Fetch alerts
    generator.fetch_alerts()
    
    # Generate reports
    generator.generate_html_report('security-report.html')
    generator.generate_markdown_report('security-report.md')
    
    print("\n✓ Report generation completed successfully!")
    print("📊 Reports saved to: security-report.html and security-report.md")

if __name__ == '__main__':
    main()
