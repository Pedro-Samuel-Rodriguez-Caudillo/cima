# Product Guidelines - CIMA (Real Estate Platform)

## Prose Style & Tone
- **Professional Minimalist:** The platform's voice is precise, elegant, and sophisticated, avoiding fluff or overly emotive language.
- **Swedish Design Influence:** Content should be structured with clarity and functionality in mind—direct, data-driven, and devoid of clutter, yet retaining a premium, high-end feel.
- **Guideline:** "Speak like an architect explaining a blueprint: clear, confident, and focused on essential details."

## Visual Design Identity
- **High-Contrast & Spatial:** The UI leverages significant white space to allow high-quality property photography to stand out as the primary hero content. Typography should be crisp and bold.
- **Nordic Palette with Anchor:**
    - **Base:** Soft grays, crisp whites, and natural muted tones to create a clean, "engineered" atmosphere.
    - **Primary Anchor:** A **Deep Dark Blue** (Midnight/Navy) serves as the defining brand color, used for primary actions, headers, or key focal points to provide weight and sophistication.
- **Strict Hierarchy:** Data presentation relies on grid-based layouts and clean lines, ensuring that technical specifications (price, area, features) are immediately readable.

## User Personas & Permissions (Admin Section)
- **Primary Admin User:** The **Architect**. The entire administrative experience is optimized for their specific workflow (managing listings and tracking their portfolio).
- **Role Hierarchy:**
    - **Standard Architect:** Can manage their own listings and view their personal statistics.
    - **Admin Architect:** A privileged role capable of managing platform-wide settings, user roles, and global content, in addition to standard architect tasks.
- **Tone for Admin:** Utility-focused but sophisticated. It should feel like a professional tool, not a complex configuration panel.

## UX & Navigation Patterns
- **Unified Context-Aware Header:** To maintain the "Hybrid App" feel, the application utilizes a single responsive header component that intelligently adapts its contents.
    - **Public State:** Minimal links (Search, About, Contact) to reduce distraction.
    - **Authenticated State:** Introduces management tools (Dashboard, My Listings) seamlessly. Higher-level Admin links are revealed only to those with appropriate permissions.
- **Immersive Interaction:** Transitions between pages should be fluid (SPA-like), avoiding full page reloads to maintain the premium, app-like feel.
