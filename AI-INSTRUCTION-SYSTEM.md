# AI Instruction Module System

This workspace uses a **modular AI instruction system** that automatically consolidates instruction files while keeping them easily editable and maintainable.

## 📋 **How It Works**

### ✅ **Requirements Met:**
- **R1**: Automatic loading - No manual intervention needed each session
- **R2**: Efficient - Only reads files when they change, not every session

### 🔄 **Automatic Process:**
1. **Modular Files**: Edit `.ai-*.md` files in the workspace root
2. **Auto-Detection**: VS Code tasks detect file changes automatically  
3. **Consolidation**: Script merges content into `.github/copilot-instructions.md`
4. **Auto-Loading**: GitHub Copilot automatically reads the consolidated file

## 📁 **File Structure**

```
AgentAsAService/
├── .github/
│   └── copilot-instructions.md     # ✅ Auto-consolidated (GitHub Copilot reads this)
├── .vscode/
│   ├── settings.json               # ✅ Workspace configuration
│   └── tasks.json                  # ✅ Auto-consolidation tasks
├── scripts/
│   └── consolidate-ai-instructions.ps1  # ✅ Consolidation script
├── .ai-quick-reference.md          # ✅ Editable module
├── .ai-workflow-config.md          # ✅ Editable module
└── .ai-[name].md                   # ✅ Add more modules here
```

## 🛠 **Usage**

### **Adding New Instruction Modules:**
1. Create a new file: `.ai-[descriptive-name].md`
2. Add your instructions in Markdown format
3. Save the file - consolidation happens automatically

### **Editing Existing Modules:**
1. Edit any `.ai-*.md` file directly
2. Save the file - changes are auto-consolidated
3. The consolidated content appears in `.github/copilot-instructions.md`

### **Manual Consolidation:**
```powershell
# Run manually if needed
pwsh -ExecutionPolicy Bypass -File "./scripts/consolidate-ai-instructions.ps1" -Verbose
```

## 🎯 **Current Modules**

- **`.ai-quick-reference.md`**: Quick reference card for AI tools
- **`.ai-workflow-config.md`**: Comprehensive workflow and tool usage guidelines

## ⚙️ **VS Code Integration**

### **Automatic Tasks:**
- **On Folder Open**: Consolidates instructions automatically
- **File Watching**: Detects changes and re-consolidates
- **Background Processing**: Runs silently without interrupting work

### **Manual Tasks:**
- `Ctrl+Shift+P` → "Tasks: Run Task" → "Consolidate AI Instructions"

## 📖 **Documentation References**

- [GitHub Copilot Custom Instructions](https://docs.github.com/en/copilot/customizing-copilot/adding-custom-instructions-for-github-copilot)
- [VS Code Tasks](https://code.visualstudio.com/docs/editor/tasks)
- [VS Code Workspace Settings](https://code.visualstudio.com/docs/getstarted/settings)

## 🔧 **Troubleshooting**

### **Instructions Not Loading:**
1. Check if `.github/copilot-instructions.md` was updated
2. Run manual consolidation: `pwsh ./scripts/consolidate-ai-instructions.ps1`
3. Restart VS Code if needed

### **File Watching Not Working:**
1. Check VS Code tasks are running: View → Terminal → "Tasks" tab
2. Run "Watch AI Instructions" task manually if needed

### **Script Execution Issues:**
1. Ensure PowerShell execution policy allows scripts:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

## ✨ **Benefits**

- **✅ Modular**: Keep instruction topics in separate, focused files
- **✅ Automatic**: No manual steps required for loading
- **✅ Efficient**: Only processes changes, not every session  
- **✅ Extensible**: Easy to add new instruction modules
- **✅ Version Controlled**: All instruction files are tracked in Git
- **✅ Reliable**: Uses GitHub Copilot's built-in auto-loading mechanism
