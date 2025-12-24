---
description: 
---

### **Workflow C: Database Migration**

*Input: Schema Change Spec*

1.  **SQL Generation (Archivist):** Write `V[#]__[Description].sql` migration script.
2.  **Model Update (Forge-Master):** Update corresponding C# Entity class in `RuneAndRust.Core`.
3.  **Seeding (Archivist):** Generate `INSERT` statements for initial data (e.g., default Item definitions).