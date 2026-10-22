// Kiji's static pages initialize Fluent without starting Blazor.
const script = document.querySelector('script[data-fluent-module]');
const { beforeStart } = await import(script.dataset.fluentModule);
beforeStart({});
