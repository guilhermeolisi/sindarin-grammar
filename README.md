# sindarin-grammar

Canonical TextMate grammar, language configuration and snippets for the
**Sindarin** crystal-diffraction input language (`.sin`).

This repository is the single source of truth for Sindarin syntax. It is
consumed as a **git submodule** by:

- [`sindarin-for-vscode`](https://github.com/guilhermeolisi/sindarin-for-vscode)
  — the Visual Studio Code extension (mounted at `grammar/`).
- [`GOSAvaloniaControl`](https://github.com/guilhermeolisi/GOSAvaloniaControl)
  — the Nimloth AvaloniaEdit control (mounted at
  `src/SindarinTextMate/Grammars/sindarin/`).

Edit the grammar **only here**. After changing it, update the submodule
pointer in each consumer:

```sh
git -C <consumer> submodule update --remote grammar
```

## Contents

| File | Purpose |
| --- | --- |
| `syntaxes/sindarin.tmLanguage.json` | TextMate grammar (`source.sin`) |
| `language-configuration.json` | Brackets, comments, auto-closing pairs |
| `snippets/sindarin.code-snippets` | Editor snippets |
| `package.json` / `package.nls.json` | Grammar bundle manifest |
| `cgmanifest.json` | Component governance manifest |
