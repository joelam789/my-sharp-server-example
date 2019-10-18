
const del = require('del');
const gulp = require('gulp');
const sourcemap = require('gulp-sourcemaps');
const webserver = require('gulp-webserver');

const fs = require("fs");
const abundle = require('aurelia-bundler').bundle;

const tsconfig = require('gulp-typescript').createProject('tsconfig.json');

gulp.task('clear-all', async () => {
    del.sync(["./dist/**/*"]);
});

gulp.task('copy-lib', function () {
    return gulp.src([
        "./jspm_packages/**/*"
        ])
        .pipe(gulp.dest("./dist/jspm_packages/"));
});

gulp.task('copy-index', function () {
    return gulp.src([
        "./index.html"
        ])
        .pipe(gulp.dest("./dist/"));
});

gulp.task('copy-module-config', function () {
    return gulp.src(["./config.js"]).pipe(gulp.dest("./dist/"));
});

gulp.task('backup-module-config', function () {
    return gulp.src(["./config.js"]).pipe(gulp.dest("./tmp/"));
});

gulp.task('restore-module-config', function () {
    return gulp.src(["./tmp/config.js"]).pipe(gulp.dest("./"));
});

gulp.task('create-fake-bundle', async () => {
    //fs.closeSync(fs.openSync('./dist/bundle-app.js', 'a'));
    //fs.closeSync(fs.openSync('./dist/bundle-vendor.js', 'a'));
    fs.writeFileSync('./dist/bundle-app.js', "console.log('fake app bundle is loaded');", 'utf8');
    fs.writeFileSync('./dist/bundle-vendor.js', "console.log('fake vendor bundle is loaded');", 'utf8');
});

gulp.task('copy-template', function () {
    return gulp.src([
        "./src/**/*.html",
        "./src/**/*.css"
        ])
        .pipe(gulp.dest("./dist/"));
});

gulp.task('copy-resource', function () {
    return gulp.src([
        "./res/**/*"
        ])
        .pipe(gulp.dest("./dist/"));
});

gulp.task("transpile", () => {
    return gulp.src([
        //"./typings/index.d.ts",
        "./src/**/*.ts"
    ])
    .pipe(sourcemap.init({ loadMaps: true }))
    .pipe(tsconfig()).js
    .pipe(sourcemap.write("./", {includeContent: false, sourceRoot: '../src'}))
    .pipe(gulp.dest("./dist/"));
});

//gulp.task("minify-js", () => {
//    return gulp.src([
//        "./dist/**/*.js"
//    ])
//    .pipe(minifyjs({
//        ext: { min: '.js' },
//        mangle: false,
//        noSource: true,
//        exclude: ['node_modules', 'jspm_packages'],
//        ignoreFiles: ['-min.js']
//    }))
//    .pipe(gulp.dest('./dist'));
//});

//gulp.task("minify-html", () => {
//    return gulp.src([
//        "./src/**/*.html"
//    ])
//    .pipe(minifyhtml({
//        collapseWhitespace: true,
//        removeComments: true
//    }))
//    .pipe(gulp.dest('./dist'));
//});

gulp.task("watch", function () {
    return gulp.watch(["./index.html", "./app-config.json", "./src/*.*", "./res/**/*"],
    gulp.series("build-main"));
});

gulp.task("apply-app-config", async() => {
    let appConfig = JSON.parse(fs.readFileSync('./app-config.json', 'utf8'));
    let configCode = "window.mainAppConfig = JSON.parse('" + JSON.stringify(appConfig) + "');";
    fs.writeFileSync('./dist/js/app-config.js', configCode, 'utf8');
});

gulp.task("build-main", gulp.series(
             'copy-index',
             'copy-template',
             'copy-resource',
             'transpile',
             'apply-app-config')
             );

gulp.task("build-and-watch", gulp.series(
             'clear-all',
            ['copy-lib', 'copy-index', 'copy-module-config', 'create-fake-bundle'],
             'copy-template',
             'copy-resource',
             'transpile',
             'apply-app-config',
             'watch')
             );

gulp.task("bundle", async () => {
    let appConfig = JSON.parse(fs.readFileSync('./app-config.json', 'utf8'));
    let bundleConfig = JSON.parse(fs.readFileSync('./bundle.json', 'utf8'));
    return abundle(bundleConfig);
});

gulp.task('clean-up', async () => {
    del.sync(["dist/*.js.map"]);
    del.sync(["dist/*.js", "!dist/config.js", "!dist/bundle-app.js", "!dist/bundle-vendor.js"]);
    del.sync(["dist/*.html", "!dist/index.html"]);
    del.sync(["tmp/**/*"]); //del.sync(["./tmp"]);
    try { del.sync(["tmp/**/*"]); del.sync(["./tmp"]); } catch(err) { }
    del.sync(["dist/jspm_packages/github/**/*"]);
    del.sync(["dist/jspm_packages/npm/**/*", 
                "!dist/jspm_packages/npm/aurelia-dialog*", "!dist/jspm_packages/npm/aurelia-dialog*/*.js" // ...
             ]);
});


gulp.task("release", gulp.series(
             'clear-all',
            ['copy-lib', 'copy-module-config'],
             'copy-template',
             'copy-resource',
             'transpile',
             'backup-module-config',
             'bundle',
             'copy-module-config',
             'restore-module-config',
             'apply-app-config',
             'copy-index',
             'clean-up'
             )
             );

gulp.task("build-only", gulp.series(
             'clear-all',
            ['copy-lib', 'copy-index', 'copy-module-config', 'create-fake-bundle'],
             'copy-template',
             'copy-resource',
             'transpile',
             'apply-app-config')
             );

gulp.task('start', function() {
  gulp.src('./')
  .pipe(webserver({
      host: "0.0.0.0",
      port: 9000
    }));
});

//gulp.task('default', ['build-and-watch']);
gulp.task('default', gulp.series('build-and-watch'));
