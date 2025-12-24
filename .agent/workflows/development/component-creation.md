---
description: 
---

### **Workflow B: UI Component Creation**

1.  **ViewModel Construction (Rune-Scribe):** Create `[Name]ViewModel.cs` inheriting `ViewModelBase`. Define `ReactiveCommand`s for interactions.
2.  **View Construction (Rune-Scribe):** Create `[Name]View.axaml`. Set up `Grid`/`StackPanel` layout and bindings.
3.  **Integration (Rune-Scribe):** Add DataTemplate to `App.axaml`.
4.  **Navigation (Forge-Master):** Register route in `NavigationService`.