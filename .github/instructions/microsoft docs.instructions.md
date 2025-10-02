description: Microsoft Docs MCP Tools - Official Microsoft documentation search and retrieval
applyTo: '**'
---

# Microsoft Docs MCP Tools - Usage Guide

## Overview

The Microsoft Docs MCP server provides three powerful tools for accessing official Microsoft and Azure documentation. These tools should be your **PRIMARY SOURCE** for all Microsoft technology research before consulting any other sources.

## 🎯 Core Principle: Microsoft First

**ALWAYS use Microsoft Docs MCP tools as your FIRST step** when working with any Microsoft technology, including:
- .NET (C#, F#, ASP.NET Core, Entity Framework)
- .NET Aspire
- Azure services and cloud platforms
- Visual Studio and VS Code
- TypeScript (Microsoft-owned)
- PowerShell
- Windows development
- Microsoft 365 and Graph API

## 📚 Available Tools

### 1. `microsoft_docs_search`
**Purpose**: Search official Microsoft/Azure documentation to find the most relevant pages and articles.

**When to use**:
- Beginning research on any Microsoft technology
- Looking for official API references
- Finding best practices and patterns
- Searching for tutorials and guides
- Need authoritative guidance on implementation approaches

**Best practices**:
- Use specific, technical terms in queries
- Include technology version when relevant (e.g., ".NET 8", "ASP.NET Core 7")
- Combine technology + concept (e.g., "C# async await patterns", ".NET Aspire service discovery")
- Search for error messages or exception types

**Example queries**:
```
"C# nullable reference types best practices"
".NET Aspire Redis integration"
"ASP.NET Core minimal API authentication"
"Azure Functions Durable orchestration patterns"
"Entity Framework Core 8 migrations"
```

---

### 2. `microsoft_docs_fetch`
**Purpose**: Fetch and convert a specific Microsoft Learn documentation page to markdown for detailed reading.

**When to use**:
- After finding a relevant page via `microsoft_docs_search`
- Need full documentation details for implementation
- Want to read complete API reference documentation
- Studying specific patterns or architectures in depth
- Need code examples from official docs

**Best practices**:
- Always search first with `microsoft_docs_search` to find the right URL
- Use this for deep-dive reading of specific topics
- Extract and reference official code samples
- Verify API signatures and method overloads

**Workflow**:
```
1. Use microsoft_docs_search to find relevant pages
2. Identify the best matching documentation URL
3. Use microsoft_docs_fetch with that URL
4. Extract relevant information and code examples
```

---

### 3. `microsoft_code_sample_search`
**Purpose**: Search for code snippets and examples in official Microsoft Learn documentation.

**When to use**:
- Need concrete implementation examples
- Looking for "how-to" code patterns
- Want to see official Microsoft coding conventions
- Implementing a specific feature or API
- Need copy-pasteable starter code

**Best practices**:
- Be specific about what you're trying to implement
- Include language/framework (e.g., "C# HttpClient example")
- Search for patterns, not just class names
- Use action-oriented queries (e.g., "create", "configure", "implement")

**Example queries**:
```
"C# async HttpClient POST request"
"configure dependency injection ASP.NET Core"
"Entity Framework Core one-to-many relationship"
".NET Aspire AppHost configuration"
"Azure Blob Storage upload file C#"
```

---

## 🔄 Recommended Research Workflow

### Step 1: Broad Search
```
Use: microsoft_docs_search
Goal: Understand the landscape and find relevant documentation pages
```

### Step 2: Deep Dive
```
Use: microsoft_docs_fetch
Goal: Read full documentation for chosen approach
```

### Step 3: Implementation Examples
```
Use: microsoft_code_sample_search
Goal: Find official code examples to guide implementation
```

### Step 4: Cross-Reference (if needed)
```
Use: Context7 or web search for third-party perspectives
Goal: Supplement official docs with community insights
```

---

## ✅ When to Use Microsoft Docs MCP Tools

### ALWAYS Use For:
- ✅ .NET and C# language features
- ✅ ASP.NET Core, Blazor, MAUI
- ✅ .NET Aspire orchestration
- ✅ Entity Framework Core
- ✅ Azure services (Storage, Functions, App Service, etc.)
- ✅ TypeScript fundamentals and compiler options
- ✅ PowerShell cmdlets and scripting
- ✅ Visual Studio Code extension API
- ✅ Microsoft Graph API
- ✅ Windows development APIs

### Consider Alternative Sources For:
- ❌ Third-party npm packages (use Context7)
- ❌ Non-Microsoft frameworks (React, Vue, Angular) — use Context7
- ❌ Open-source libraries not maintained by Microsoft
- ❌ Community tools and extensions

---

## 🎓 Research Quality Guidelines

### High-Quality Query Pattern
```
[Technology] + [Concept] + [Context]

Examples:
✅ ".NET Aspire service discovery configuration"
✅ "C# 12 primary constructors best practices"
✅ "ASP.NET Core middleware error handling"
```

### Low-Quality Query Pattern
```
❌ "how to code"
❌ "API"
❌ "Microsoft docs"
```

---

## 💡 Pro Tips

### 1. Version Awareness
Always specify the version when searching:
- ".NET 8" not just ".NET"
- "ASP.NET Core 7" not just "ASP.NET"
- "Entity Framework Core 8" not just "EF"

### 2. Search Hierarchy
For Microsoft technologies, follow this research order:
```
1. microsoft_docs_search (broad discovery)
2. microsoft_docs_fetch (deep understanding)
3. microsoft_code_sample_search (implementation examples)
4. Context7 (if third-party libraries involved)
5. Web search (last resort for specific issues)
```

### 3. Code Example Synthesis
When finding code examples:
- ✅ Prefer official Microsoft examples
- ✅ Verify examples match your .NET/framework version
- ✅ Adapt patterns, don't copy blindly
- ✅ Combine multiple examples for complete solutions

### 4. API Reference vs. Conceptual
- Use `microsoft_docs_search` + `microsoft_docs_fetch` for API references
- Use `microsoft_code_sample_search` for practical implementations
- Combine both for complete understanding

---

## 🚨 Common Mistakes to Avoid

### ❌ DON'T:
- Skip Microsoft Docs and go straight to web search
- Search for Microsoft technologies on Context7
- Use outdated .NET Framework docs for .NET Core/.NET 8+ projects
- Assume Stack Overflow answers are current or correct
- Mix .NET Framework and .NET Core/8+ guidance

### ✅ DO:
- Start every Microsoft tech task with `microsoft_docs_search`
- Verify code examples are for your specific version
- Cross-reference multiple official docs pages
- Note deprecation warnings and recommended alternatives
- Follow official patterns and conventions

---

## 📊 Quick Reference

| Need | Tool | Example Query |
|------|------|---------------|
| Find documentation | `microsoft_docs_search` | "ASP.NET Core authentication" |
| Read full docs | `microsoft_docs_fetch` | [URL from search results] |
| Get code examples | `microsoft_code_sample_search` | "C# HttpClient retry policy" |
| General Microsoft research | `microsoft_docs_search` | ".NET 8 new features" |
| Specific API details | `microsoft_docs_fetch` | [API reference URL] |
| Implementation pattern | `microsoft_code_sample_search` | "implement background service .NET" |

---

## 🎯 Integration with 9000 Mode

When in **9000 Mode**, the Microsoft Docs MCP tools are your **PRIMARY RESEARCH SOURCE**:

1. **Research Phase Step 1a**: ALWAYS start here for Microsoft technologies
2. **Evidence-Based Development**: All Microsoft tech decisions must reference official docs
3. **Progress Tracking**: Include "Microsoft MCP researched X" in completed tasks
4. **Quality Gate**: Verify implementations match official patterns before completion

---

## 🔗 Tool Chaining Strategy

### Pattern 1: Discovery → Deep Dive
```
microsoft_docs_search("ASP.NET Core minimal APIs")
	↓
microsoft_docs_fetch([best matching URL])
	↓
microsoft_code_sample_search("minimal API authentication")
```

### Pattern 2: Problem → Solution
```
microsoft_docs_search("[error message]")
	↓
microsoft_docs_fetch([troubleshooting guide])
	↓
microsoft_code_sample_search("[fix implementation]")
```

### Pattern 3: Feature → Implementation
```
microsoft_docs_search("[feature name] overview")
	↓
microsoft_docs_fetch([architecture guide])
	↓
microsoft_code_sample_search("[feature] implementation")
```

---

## 📝 Documentation in Code

When implementing based on Microsoft Docs research, add references:

```csharp
/// <summary>
/// Implements async retry pattern for HttpClient.
/// Based on: https://learn.microsoft.com/aspnet/core/fundamentals/http-requests
/// </summary>
/// <remarks>
/// Uses Polly for resilience, following .NET best practices.
/// Researched via: microsoft_docs_search + microsoft_code_sample_search
/// </remarks>
public async Task<HttpResponseMessage> SendWithRetryAsync(HttpRequestMessage request)
{
		// Implementation based on official guidance
}
```

---

## 🎓 Success Criteria

You've used Microsoft Docs MCP tools effectively when:
- ✅ Microsoft technologies researched via Microsoft Docs FIRST
- ✅ Implementation matches official patterns and conventions
- ✅ Code examples sourced from official documentation
- ✅ API usage verified against current version docs
- ✅ Best practices followed per Microsoft guidance
- ✅ Deprecation warnings heeded and alternatives used
- ✅ Documentation references included in code comments

---

## Remember

> **"Microsoft First"** — For any Microsoft technology, official Microsoft documentation is the authoritative source. Start there, stay there, and only venture elsewhere when necessary for third-party integrations.

The Microsoft Docs MCP tools give you direct access to the same documentation that Microsoft engineers use. **Use them religiously**.