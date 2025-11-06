---
title: "WinGet Studio changelog"
description: >-
  A log of the changes for releases of WinGet Studio.
ms.topic: whats-new
ms.date: 01/01/2025
---

# Changelog

<!-- markdownlint-disable-file MD033 -->

<!--
    Helpful docs snippets

    You can use the following snippets in this file to make authoring new change entries and
    releases easier.

    docs-changelog-entry-single-issue-pr: Adds a new changelog list entry with one related issue
    and PR. Use this when a change entry only has one related issue and/or pull request. Always
    use this snippet under a change kind heading, like 'Added' or 'Changed.'

    docs-changelog-entry-multi-issue-pr: Adds a new changelog list entry with sub-lists for issues
    and PRs. Use this when a change entry has more than one related issue and/or pull request.
    Always use this snippet under a change kind heading, like 'Added' or 'Changed.'

    docs-changelog-release-heading: Adds a new changelog release heading, following our existing
    format. Use this when a new release is created to ensure that the new release heading has the
    required links and synopsis.

    docs-gh-link: Adds a new link to an issue or pull request on GitHub. Use this when adding a new
    work item link reference and it will automatically construct the URL and reference link ID for
    you from the work item ID.
-->

All notable changes to WinGet Studio are documented in this file. The format is based on
[Keep a Changelog][m1], and WinGet Studio adheres to [Semantic Versioning][m2].

<!-- Meta links -->
[m1]: https://keepachangelog.com/en/1.1.0/
[m2]: https://semver.org/spec/v2.0.0.html

## [v0.100.302.0][release-v0.100.302.0] v0.100.302.0 - 2025-10-28

This section includes a summary of changes for the `v0.100.302.0` release. For the full list of
changes in this release, see the [diff on GitHub][compare-v0.100.302.0].

<!-- Release links -->
[release-v0.100.302.0]: https://github.com/microsoft/winget-studio/releases/tag/v0.100.302.0 "Link to the WinGet Studio v0.100.302.0 release on GitHub"
[compare-v0.100.302.0]: https://github.com/microsoft/winget-studio/compare/v0.100.293.0...v0.100.302.0

### Added

- Added cache manager to improve performance when working with DSC resources and configuration
  files. The cache manager reduces redundant operations and speeds up resource discovery.

  <details><summary>Related work items</summary>

  - Issues: _None_
  - PRs: [#119][#119]

  </details>

- Added support for cancellation tokens throughout the application. You can now cancel long-running
  applying a configuration set operation.

  <details><summary>Related work items</summary>

  - Issues: _None_
  - PRs: [#122][#122]

  </details>

- Added shimmer loading control to provide visual feedback during asynchronous operations.

  <details><summary>Related work items</summary>

  - Issues: _None_
  - PRs: [#120][#120]

  </details>

- Added default schema generator support for DSC resources. This feature automatically generates  
  a default YAML input based on the schema for DSC resources.

  <details><summary>Related work items</summary>

  - Issues: _None_
  - PRs: [#118][#118]

  </details>

### Changed

- Improved session state management by moving capture and restore logic to dedicated session state
  aware classes. This change provides better isolation and reliability when working with
  configurations that modify system state.

  <details><summary>Related work items</summary>

  - Issues: _None_
  - PRs: [#123][#123]

  </details>

- Unit details are now loaded on demand when expanding a unit in the configuration tree view. This
  change significantly improves initial load performance for configurations with many units, as
  detailed information is only fetched when needed.

  <details><summary>Related work items</summary>

  - Issues: _None_
  - PRs: [#124][#124]

  </details>

### Fixed

- Removed unintended editor focus behavior that could cause focus to jump unexpectedly when
  navigating through the configuration interface.

  <details><summary>Related work items</summary>

  - Issues: _None_
  - PRs: [#121][#121]

  </details>

<!-- Issue and PR links -->
[#118]: https://github.com/microsoft/winget-studio/pull/118
[#119]: https://github.com/microsoft/winget-studio/pull/119
[#120]: https://github.com/microsoft/winget-studio/pull/120
[#121]: https://github.com/microsoft/winget-studio/pull/121
[#122]: https://github.com/microsoft/winget-studio/pull/122
[#123]: https://github.com/microsoft/winget-studio/pull/123
[#124]: https://github.com/microsoft/winget-studio/pull/124
