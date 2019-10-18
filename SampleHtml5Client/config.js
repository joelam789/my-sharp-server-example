System.config({
  baseURL: "./",
  defaultJSExtensions: true,
  transpiler: false,
  paths: {
    "github:*": "jspm_packages/github/*",
    "npm:*": "jspm_packages/npm/*"
  },

  map: {
    "aurelia-bootstrapper": "npm:aurelia-bootstrapper@2.3.3",
    "aurelia-dialog": "npm:aurelia-dialog@2.0.0-rc.8",
    "aurelia-event-aggregator": "npm:aurelia-event-aggregator@1.0.3",
    "aurelia-framework": "npm:aurelia-framework@1.3.1",
    "aurelia-i18n": "npm:aurelia-i18n@3.0.0-beta.7",
    "aurelia-pal-browser": "npm:aurelia-pal-browser@1.8.1",
    "aurelia-router": "npm:aurelia-router@1.7.1",
    "core-js": "npm:core-js@3.3.2",
    "i18next": "npm:i18next@17.2.0",
    "i18next-xhr-backend": "npm:i18next-xhr-backend@3.2.0",
    "nprogress": "github:rstacruz/nprogress@0.2.0",
    "text": "github:systemjs/plugin-text@0.0.11",
    "github:jspm/nodelibs-assert@0.1.0": {
      "assert": "npm:assert@1.5.0"
    },
    "github:jspm/nodelibs-buffer@0.1.1": {
      "buffer": "npm:buffer@5.4.3"
    },
    "github:jspm/nodelibs-os@0.1.0": {
      "os-browserify": "npm:os-browserify@0.1.2"
    },
    "github:jspm/nodelibs-path@0.1.0": {
      "path-browserify": "npm:path-browserify@0.0.0"
    },
    "github:jspm/nodelibs-process@0.1.2": {
      "process": "npm:process@0.11.10"
    },
    "github:jspm/nodelibs-util@0.1.0": {
      "util": "npm:util@0.10.3"
    },
    "github:jspm/nodelibs-vm@0.1.0": {
      "vm-browserify": "npm:vm-browserify@0.0.4"
    },
    "github:rstacruz/nprogress@0.2.0": {
      "css": "github:systemjs/plugin-css@0.1.37"
    },
    "npm:@babel/runtime@7.6.3": {
      "regenerator-runtime": "npm:regenerator-runtime@0.13.3"
    },
    "npm:assert@1.5.0": {
      "assert": "github:jspm/nodelibs-assert@0.1.0",
      "buffer": "github:jspm/nodelibs-buffer@0.1.1",
      "object-assign": "npm:object-assign@4.1.1",
      "process": "github:jspm/nodelibs-process@0.1.2",
      "util": "npm:util@0.10.3"
    },
    "npm:aurelia-binding@2.5.0": {
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-task-queue": "npm:aurelia-task-queue@1.3.3"
    },
    "npm:aurelia-bootstrapper@2.3.3": {
      "aurelia-event-aggregator": "npm:aurelia-event-aggregator@1.0.3",
      "aurelia-framework": "npm:aurelia-framework@1.3.1",
      "aurelia-history": "npm:aurelia-history@1.2.1",
      "aurelia-history-browser": "npm:aurelia-history-browser@1.4.0",
      "aurelia-loader-default": "npm:aurelia-loader-default@1.2.1",
      "aurelia-logging-console": "npm:aurelia-logging-console@1.1.1",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-pal-browser": "npm:aurelia-pal-browser@1.8.1",
      "aurelia-polyfills": "npm:aurelia-polyfills@1.3.4",
      "aurelia-router": "npm:aurelia-router@1.7.1",
      "aurelia-templating": "npm:aurelia-templating@1.10.2",
      "aurelia-templating-binding": "npm:aurelia-templating-binding@1.5.3",
      "aurelia-templating-resources": "npm:aurelia-templating-resources@1.12.0",
      "aurelia-templating-router": "npm:aurelia-templating-router@1.4.0"
    },
    "npm:aurelia-dependency-injection@1.5.1": {
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2"
    },
    "npm:aurelia-dialog@2.0.0-rc.8": {
      "aurelia-binding": "npm:aurelia-binding@2.5.0",
      "aurelia-dependency-injection": "npm:aurelia-dependency-injection@1.5.1",
      "aurelia-framework": "npm:aurelia-framework@1.3.1",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-templating": "npm:aurelia-templating@1.10.2"
    },
    "npm:aurelia-event-aggregator@1.0.3": {
      "aurelia-logging": "npm:aurelia-logging@1.5.2"
    },
    "npm:aurelia-framework@1.3.1": {
      "aurelia-binding": "npm:aurelia-binding@2.5.0",
      "aurelia-dependency-injection": "npm:aurelia-dependency-injection@1.5.1",
      "aurelia-loader": "npm:aurelia-loader@1.0.2",
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-path": "npm:aurelia-path@1.1.5",
      "aurelia-task-queue": "npm:aurelia-task-queue@1.3.3",
      "aurelia-templating": "npm:aurelia-templating@1.10.2"
    },
    "npm:aurelia-history-browser@1.4.0": {
      "aurelia-history": "npm:aurelia-history@1.2.1",
      "aurelia-pal": "npm:aurelia-pal@1.8.2"
    },
    "npm:aurelia-i18n@3.0.0-beta.7": {
      "aurelia-binding": "npm:aurelia-binding@2.5.0",
      "aurelia-dependency-injection": "npm:aurelia-dependency-injection@1.5.1",
      "aurelia-event-aggregator": "npm:aurelia-event-aggregator@1.0.3",
      "aurelia-loader": "npm:aurelia-loader@1.0.2",
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-templating": "npm:aurelia-templating@1.10.2",
      "aurelia-templating-resources": "npm:aurelia-templating-resources@1.12.0",
      "i18next": "npm:i18next@14.1.1"
    },
    "npm:aurelia-loader-default@1.2.1": {
      "aurelia-loader": "npm:aurelia-loader@1.0.2",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2"
    },
    "npm:aurelia-loader@1.0.2": {
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-path": "npm:aurelia-path@1.1.5"
    },
    "npm:aurelia-logging-console@1.1.1": {
      "aurelia-logging": "npm:aurelia-logging@1.5.2"
    },
    "npm:aurelia-metadata@1.0.6": {
      "aurelia-pal": "npm:aurelia-pal@1.8.2"
    },
    "npm:aurelia-pal-browser@1.8.1": {
      "aurelia-pal": "npm:aurelia-pal@1.8.2"
    },
    "npm:aurelia-polyfills@1.3.4": {
      "aurelia-pal": "npm:aurelia-pal@1.8.2"
    },
    "npm:aurelia-route-recognizer@1.3.2": {
      "aurelia-path": "npm:aurelia-path@1.1.5"
    },
    "npm:aurelia-router@1.7.1": {
      "aurelia-dependency-injection": "npm:aurelia-dependency-injection@1.5.1",
      "aurelia-event-aggregator": "npm:aurelia-event-aggregator@1.0.3",
      "aurelia-history": "npm:aurelia-history@1.2.1",
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-path": "npm:aurelia-path@1.1.5",
      "aurelia-route-recognizer": "npm:aurelia-route-recognizer@1.3.2"
    },
    "npm:aurelia-task-queue@1.3.3": {
      "aurelia-pal": "npm:aurelia-pal@1.8.2"
    },
    "npm:aurelia-templating-binding@1.5.3": {
      "aurelia-binding": "npm:aurelia-binding@2.5.0",
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-templating": "npm:aurelia-templating@1.10.2"
    },
    "npm:aurelia-templating-resources@1.12.0": {
      "aurelia-binding": "npm:aurelia-binding@2.5.0",
      "aurelia-dependency-injection": "npm:aurelia-dependency-injection@1.5.1",
      "aurelia-loader": "npm:aurelia-loader@1.0.2",
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-path": "npm:aurelia-path@1.1.5",
      "aurelia-task-queue": "npm:aurelia-task-queue@1.3.3",
      "aurelia-templating": "npm:aurelia-templating@1.10.2"
    },
    "npm:aurelia-templating-router@1.4.0": {
      "aurelia-binding": "npm:aurelia-binding@2.5.0",
      "aurelia-dependency-injection": "npm:aurelia-dependency-injection@1.5.1",
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-path": "npm:aurelia-path@1.1.5",
      "aurelia-router": "npm:aurelia-router@1.7.1",
      "aurelia-templating": "npm:aurelia-templating@1.10.2"
    },
    "npm:aurelia-templating@1.10.2": {
      "aurelia-binding": "npm:aurelia-binding@2.5.0",
      "aurelia-dependency-injection": "npm:aurelia-dependency-injection@1.5.1",
      "aurelia-loader": "npm:aurelia-loader@1.0.2",
      "aurelia-logging": "npm:aurelia-logging@1.5.2",
      "aurelia-metadata": "npm:aurelia-metadata@1.0.6",
      "aurelia-pal": "npm:aurelia-pal@1.8.2",
      "aurelia-path": "npm:aurelia-path@1.1.5",
      "aurelia-task-queue": "npm:aurelia-task-queue@1.3.3"
    },
    "npm:buffer@5.4.3": {
      "base64-js": "npm:base64-js@1.3.1",
      "ieee754": "npm:ieee754@1.1.13"
    },
    "npm:core-js@3.3.2": {
      "fs": "github:jspm/nodelibs-fs@0.1.2",
      "os": "github:jspm/nodelibs-os@0.1.0",
      "path": "github:jspm/nodelibs-path@0.1.0",
      "process": "github:jspm/nodelibs-process@0.1.2"
    },
    "npm:i18next-xhr-backend@3.2.0": {
      "@babel/runtime": "npm:@babel/runtime@7.6.3"
    },
    "npm:i18next@14.1.1": {
      "process": "github:jspm/nodelibs-process@0.1.2"
    },
    "npm:i18next@17.2.0": {
      "@babel/runtime": "npm:@babel/runtime@7.6.3",
      "process": "github:jspm/nodelibs-process@0.1.2"
    },
    "npm:inherits@2.0.1": {
      "util": "github:jspm/nodelibs-util@0.1.0"
    },
    "npm:os-browserify@0.1.2": {
      "os": "github:jspm/nodelibs-os@0.1.0"
    },
    "npm:path-browserify@0.0.0": {
      "process": "github:jspm/nodelibs-process@0.1.2"
    },
    "npm:process@0.11.10": {
      "assert": "github:jspm/nodelibs-assert@0.1.0",
      "fs": "github:jspm/nodelibs-fs@0.1.2",
      "vm": "github:jspm/nodelibs-vm@0.1.0"
    },
    "npm:regenerator-runtime@0.13.3": {
      "path": "github:jspm/nodelibs-path@0.1.0"
    },
    "npm:util@0.10.3": {
      "inherits": "npm:inherits@2.0.1",
      "process": "github:jspm/nodelibs-process@0.1.2"
    },
    "npm:vm-browserify@0.0.4": {
      "indexof": "npm:indexof@0.0.1"
    }
  }
});
