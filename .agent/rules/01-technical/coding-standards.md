---
trigger: always_on
---

## **Coding Standards**

* **Naming:** PascalCase for public members, `_camelCase` for private fields.
* **Async:** Always use `async Task`, never `async void` (except strict EventHandlers).
* **Nullability:** Enable `<Nullable>enable</Nullable>` and handle warnings strictly.
* **Project Structure:**
  *   `Services/` for logic.
  *   `ViewModels/` for state.
  *   `Views/` for XAML.
  *   `Assets/` for static files.