---
description: 
---

### **Workflow A: Feature Implementation (Spec-to-Code)**

1.  **Analysis (Forge-Master):** Read spec Description, Dependencies, and Type. Identify required Services and Models.
2.  **Contract Definition (Forge-Master):** Generate `I[Feature]Service` interface and `[Feature]Model` data class.
3.  **Test Scaffolding (QA Sentinel):** Create `[Feature]ServiceTests.cs` with failing tests for expected behavior.
4.  **Implementation (Forge-Master):** Implement the Service logic to pass tests.
5.  **Registration (Forge-Master):** Add line to `App.axaml.cs` -\> `ConfigureServices()`.