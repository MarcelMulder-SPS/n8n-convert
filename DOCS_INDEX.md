# Documentation Index

Welcome to the n8n-convert project! This index will help you navigate all available documentation.

## 🚀 Quick Start

**New to this project?** Start here:
1. [README.md](./README.md) - Overview and main documentation
2. [SETUP.md](./SETUP.md) - Step-by-step setup instructions
3. Run `./start.sh` (Linux/Mac) or `start.bat` (Windows)

## 📚 Complete Documentation List

### Essential Reading

| Document | Purpose | When to Read |
|----------|---------|--------------|
| [README.md](./README.md) | Main project documentation, architecture overview | **Start here** - First read |
| [SETUP.md](./SETUP.md) | Detailed setup and configuration guide | Before running the app |
| [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | Developer quick reference and common tasks | During development |

### Deep Dive

| Document | Purpose | When to Read |
|----------|---------|--------------|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | System architecture with visual diagrams | Understanding the design |
| [CONVERSION_NOTES.md](./CONVERSION_NOTES.md) | How n8n was converted to C# | Understanding the migration |
| [PROJECT_SUMMARY.md](./PROJECT_SUMMARY.md) | Complete project summary and statistics | Project overview |
| [UI_PREVIEW.md](./UI_PREVIEW.md) | Chat interface description and preview | Understanding the UI |

## 📖 Documentation by Topic

### Getting Started
- **Installation**: [SETUP.md](./SETUP.md) → Step 1-4
- **First Run**: [SETUP.md](./SETUP.md) → Step 5-6
- **Troubleshooting**: [SETUP.md](./SETUP.md) → Troubleshooting section

### Configuration
- **Azure OpenAI**: [SETUP.md](./SETUP.md) → Step 2
- **MCP Endpoints**: [SETUP.md](./SETUP.md) → Step 3
- **Example Config**: [src/WebAPI/appsettings.example.json](./src/WebAPI/appsettings.example.json)

### Architecture
- **System Overview**: [README.md](./README.md) → Architecture section
- **Visual Diagrams**: [ARCHITECTURE.md](./ARCHITECTURE.md)
- **Data Flow**: [ARCHITECTURE.md](./ARCHITECTURE.md) → Data Flow section
- **Agent Design**: [README.md](./README.md) → Agent System Design

### Development
- **Quick Reference**: [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
- **Project Structure**: [README.md](./README.md) → Project Structure
- **Adding Agents**: [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → Add a New Agent
- **API Endpoints**: [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → API Endpoints

### Migration from n8n
- **Conversion Guide**: [CONVERSION_NOTES.md](./CONVERSION_NOTES.md)
- **Feature Mapping**: [CONVERSION_NOTES.md](./CONVERSION_NOTES.md) → C# Implementation Mapping
- **Differences**: [CONVERSION_NOTES.md](./CONVERSION_NOTES.md) → Key Architectural Differences

### User Interface
- **UI Description**: [UI_PREVIEW.md](./UI_PREVIEW.md)
- **Features**: [UI_PREVIEW.md](./UI_PREVIEW.md) → Chat Interface Features
- **Customization**: [UI_PREVIEW.md](./UI_PREVIEW.md) → Customization

## 🎯 Documentation by Role

### For Project Managers
1. [PROJECT_SUMMARY.md](./PROJECT_SUMMARY.md) - Complete deliverables
2. [README.md](./README.md) - Project overview
3. [ARCHITECTURE.md](./ARCHITECTURE.md) - Technical architecture

### For Developers
1. [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) - Daily reference
2. [SETUP.md](./SETUP.md) - Environment setup
3. [ARCHITECTURE.md](./ARCHITECTURE.md) - System design
4. [CONVERSION_NOTES.md](./CONVERSION_NOTES.md) - Migration details

### For DevOps/SRE
1. [SETUP.md](./SETUP.md) - Deployment setup
2. [README.md](./README.md) → Running the Application
3. [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → Build & Deploy

### For End Users
1. [UI_PREVIEW.md](./UI_PREVIEW.md) - Interface guide
2. [README.md](./README.md) → Using the Application
3. [README.md](./README.md) → Example Queries

## 📝 Quick Navigation

### Common Tasks

**Want to...**
- **Run the app?** → [SETUP.md](./SETUP.md) or use `./start.sh`
- **Configure Azure OpenAI?** → [SETUP.md](./SETUP.md) → Step 2
- **Understand the architecture?** → [ARCHITECTURE.md](./ARCHITECTURE.md)
- **Add a new agent?** → [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → Add a New Agent
- **See what was converted?** → [CONVERSION_NOTES.md](./CONVERSION_NOTES.md)
- **Fix an issue?** → [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → Troubleshooting

### Code References

**Looking for...**
- **API endpoints?** → [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → API Endpoints
- **Configuration options?** → [README.md](./README.md) → Configuration
- **Agent classes?** → [README.md](./README.md) → Project Structure
- **System prompts?** → Agent source files in `src/WebAPI/Agents/`

## 📊 Documentation Statistics

| Metric | Count |
|--------|-------|
| Total Documentation Files | 7 |
| Total Lines | ~1,800+ |
| Code Examples | 30+ |
| Diagrams | 2 |
| Quick Start Guides | 2 |
| Reference Guides | 2 |
| Architecture Docs | 2 |

## 🔗 External Resources

### Microsoft Documentation
- [Agent Framework Overview](https://learn.microsoft.com/en-us/agent-framework/overview/agent-framework-overview)
- [Agent Framework GitHub](https://github.com/microsoft/agent-framework)
- [Azure OpenAI Documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

### Related Technologies
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [C# Programming Guide](https://learn.microsoft.com/en-us/dotnet/csharp/)

## 🆘 Need Help?

1. **Check troubleshooting**: [SETUP.md](./SETUP.md) → Troubleshooting
2. **Review quick reference**: [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)
3. **Check examples**: [README.md](./README.md) → Example Queries
4. **Open an issue**: Create a GitHub issue for bugs or questions

## 📱 Printable Cheat Sheets

For quick reference, print these sections:
- [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → Common Tasks
- [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → API Endpoints
- [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) → Troubleshooting

## 🎓 Learning Path

**Recommended reading order for new developers:**

1. 📖 [README.md](./README.md) - Understand the project (15 min)
2. 🔧 [SETUP.md](./SETUP.md) - Set up your environment (30 min)
3. 🏗️ [ARCHITECTURE.md](./ARCHITECTURE.md) - Learn the architecture (20 min)
4. 💡 [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) - Reference for daily use (ongoing)
5. 🔄 [CONVERSION_NOTES.md](./CONVERSION_NOTES.md) - Optional: n8n background (15 min)

**Total time to productivity**: ~1-2 hours

## 🔄 Document Updates

All documentation is version controlled with the code. Check git history for changes:
```bash
git log --oneline -- *.md
```

## 📋 Documentation Checklist

Use this to verify you've reviewed essential docs:

- [ ] Read README.md
- [ ] Followed SETUP.md
- [ ] Configured Azure OpenAI
- [ ] Successfully ran the application
- [ ] Bookmarked QUICK_REFERENCE.md
- [ ] Understood ARCHITECTURE.md
- [ ] Ready to develop!

---

**Last Updated**: February 12, 2026  
**Project Version**: 1.0.0  
**Documentation Status**: ✅ Complete
