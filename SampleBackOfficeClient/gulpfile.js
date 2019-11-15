
//const app_config = "app-config-debug";
const app_config = "app-config";

const fs = require("fs");
const del = require('del');

const gulp = require('gulp');
const rename = require('gulp-rename');
//const sequence = require('run-sequence');
const sourcemap = require('gulp-sourcemaps');
const webserver = require('gulp-webserver');

const tsconfig = require('gulp-typescript').createProject('tsconfig.json');

gulp.task('clear-all', async() => {
    del.sync(["./dist/**/*"]);
});

gulp.task('copy-html', function () {
    return gulp.src([
        "./src/**/*.html"
        ])
        .pipe(rename({dirname: ''}))
        .pipe(gulp.dest("./dist/"));
});

gulp.task('copy-resource', function () {
    return gulp.src([
        "./res/**/*"
        ])
        .pipe(gulp.dest("./dist/"));
});

gulp.task("transpile-ts", () => {
    return gulp.src([
        "./src/**/*.ts"
    ])
    .pipe(sourcemap.init({ loadMaps: true }))
    .pipe(tsconfig()).js
    .pipe(sourcemap.write("./", {includeContent: false, sourceRoot: '../src'}))
    .pipe(gulp.dest("./dist/"));
});

gulp.task("watch", function () {
    return gulp.watch(["./" + app_config + ".json", "./src/**/*", "./res/**/*"],
    gulp.series("build-main"));
});

gulp.task("apply-config", async() => {
    let appConfig = JSON.parse(fs.readFileSync('./'+ app_config +'.json', 'utf8'));
    let configCode = "window.appConfig = JSON.parse('" + JSON.stringify(appConfig) + "');";
    fs.writeFileSync('./dist/js/app-config.js', configCode, 'utf8');
});

gulp.task("build-main", gulp.series(
             'copy-html',
             'copy-resource',
             'transpile-ts',
             'apply-config'
             )
);

gulp.task("build-and-watch",gulp.series(
             'clear-all',
             'copy-html',
             'copy-resource',
             'transpile-ts',
             'apply-config',
             'watch')
);

gulp.task('start', function() {
  gulp.src('./')
  .pipe(webserver({
      host: "0.0.0.0",
      port: 9050
    }));
});

//gulp.task('default', ['build-main']);
//gulp.task('default', ['build-and-watch']);
gulp.task('default', gulp.series('build-and-watch'));
