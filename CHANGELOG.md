# Changelog

## [1.4.1](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.4.0...v1.4.1) (2025-09-03)


### Upgrades

* **deps:** update aspire monorepo to 9.4.2 ([31b7d39](https://github.com/lupusbytes/event-hub-live-explorer/commit/31b7d39130a17fd063391a50622f0fe9f71e6845))
* **deps:** update dependency mudblazor to 8.12.0 ([4b753ff](https://github.com/lupusbytes/event-hub-live-explorer/commit/4b753fffc316e53031ab0af516038d0196744b03))
* **deps:** update dependency polly to 8.6.3 ([103cae7](https://github.com/lupusbytes/event-hub-live-explorer/commit/103cae7cf97fb181be837f7e13a8613ecb23f402))

## [1.4.0](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.3.4...v1.4.0) (2025-08-09)


### New features

* add clear button ([b6f2998](https://github.com/lupusbytes/event-hub-live-explorer/commit/b6f29989ad8012c8a33f6d66ce474b66735505b8))
* add enqueued time to table ([f47d39f](https://github.com/lupusbytes/event-hub-live-explorer/commit/f47d39f64a9909e9cab3378d76632acd2d9f27a0))
* add pause/play button to toggle event streaming ([8eb6a9e](https://github.com/lupusbytes/event-hub-live-explorer/commit/8eb6a9e335c4b9fc64c00f245268d5dc2d5bfe3b))
* implement cancellation tokens to avoid race conditions when rapidly switching between event hubs ([cc72b74](https://github.com/lupusbytes/event-hub-live-explorer/commit/cc72b7470ccbfea8b3eb475e368b8f9cae85b3d1))


### Performance improvements

* use api endpoint with paged results to load historic events ([48cb5ee](https://github.com/lupusbytes/event-hub-live-explorer/commit/48cb5ee9e352a9613761509ce4104d64ec32d524))

## [1.3.4](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.3.3...v1.3.4) (2025-08-05)


### Upgrades

* **deps:** update dependency mudblazor to 8.11.0 ([d4755b9](https://github.com/lupusbytes/event-hub-live-explorer/commit/d4755b9b7d883f3102906a4068db2d3a3325ea55))
* **deps:** update dotnet monorepo to 9.0.8 ([7c69b8f](https://github.com/lupusbytes/event-hub-live-explorer/commit/7c69b8f519c24ba0cc2256584a85c7ba7f07133e))

## [1.3.3](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.3.2...v1.3.3) (2025-08-02)


### Bug fixes

* add symbols and metadata to nuget package ([d17128c](https://github.com/lupusbytes/event-hub-live-explorer/commit/d17128c8cdf7838d555de17ebe5aab45ab93360e))

## [1.3.2](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.3.1...v1.3.2) (2025-08-02)


### Upgrades

* **deps:** update aspire monorepo to 9.4.0 ([6f6af5b](https://github.com/lupusbytes/event-hub-live-explorer/commit/6f6af5bc121dd58d967d9cda56f97c7442fa1c22))

## [1.3.1](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.3.0...v1.3.1) (2025-07-27)


### Bug fixes

* **docker:** use internal port when creating prerender httpclient ([46c8328](https://github.com/lupusbytes/event-hub-live-explorer/commit/46c8328631941ed5a09235ed0288ff226b9518a8))

## [1.3.0](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.2.0...v1.3.0) (2025-07-27)


### New features

* enable prerendering ([2e721dd](https://github.com/lupusbytes/event-hub-live-explorer/commit/2e721ddfc60897c3991f99f84b4b9cd7f6c6a5e0))


### Performance improvements

* remove unnecessary statehaschanged invocation ([35a1159](https://github.com/lupusbytes/event-hub-live-explorer/commit/35a1159923c6d9dda7fe2051bbfd30c3b15d9e4d))

## [1.2.0](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.1.1...v1.2.0) (2025-07-25)


### New features

* reuse signalr websockets and use streaming for cached messages ([c89cbc0](https://github.com/lupusbytes/event-hub-live-explorer/commit/c89cbc05d1bdde60d5177dfcb9ca207d0adae9ec))
* use strongly typed signalr ([967d747](https://github.com/lupusbytes/event-hub-live-explorer/commit/967d74735fbd34ea94129b4970b3850a3349ec94))

## [1.1.1](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.1.0...v1.1.1) (2025-07-24)


### Upgrades

* **deps:** update dependency microsoft.aspnetcore.components.webassembly to 9.0.7 ([79e3ab4](https://github.com/lupusbytes/event-hub-live-explorer/commit/79e3ab4d3302b4717ff2e96d648ccf001cf1d8a3))

## [1.1.0](https://github.com/lupusbytes/event-hub-live-explorer/compare/v1.0.0...v1.1.0) (2025-07-24)


### New features

* **docker:** expose port 5000 in dockerfile ([97bcd59](https://github.com/lupusbytes/event-hub-live-explorer/commit/97bcd594e358d239c41c8f70f6a71e0d605b9cbb))
