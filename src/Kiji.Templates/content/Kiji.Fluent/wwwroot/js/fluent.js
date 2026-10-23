// Kiji's static pages initialize Fluent without starting Blazor.
// A static import keeps the render-blocking module pending until Fluent is ready.
// Resolve relative to this asset so sites hosted under a subpath also work.
import { beforeStart } from '../_content/Microsoft.FluentUI.AspNetCore.Components/Microsoft.FluentUI.AspNetCore.Components.lib.module.js';
beforeStart({});
