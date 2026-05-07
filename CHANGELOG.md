# Changelog

All notable changes to this project are documented here. The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - <YYYY-MM-DD set at release time>

### Added

- Initial release of Jellyfin and Emby SSE plugins.
- Self-hosted Jellyfin manifest at `manifest.json`.
- Self-hosted Emby package feed at `emby-packages.xml`.
- Six event types: `playing`, `progress`, `paused`, `stopped`, `session.start`, `session.end`, plus `ping` keepalive every 30 seconds.
- Bounded per-subscriber channels (capacity 100); overflow drops silently.
