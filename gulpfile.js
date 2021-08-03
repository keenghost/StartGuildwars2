const fs = require('fs')
const path = require('path')
const gulp = require('gulp')
const iconv = require('iconv-lite')
const innosetupCompiler = require('innosetup-compiler')
const packageInfo = require('./package.json')

function makeWinInstaller(options) {
  const {
    name,
    version,
    issPath,
    files,
    outputPath,
    outputFileName,
    setupResourcesPath,
    appPublisher,
    appURL,
    appId,
    appName,
    uninstallDesc,
  } = options
  const tmpIssPath = path.resolve(path.parse(issPath).dir, '_tmp_.iss')

  return new Promise(function (resolve, reject) {
    fs.readFile(issPath, null, function (err, text) {
      if (err) {
        reject(err)
        return
      }

      let str = iconv.decode(text, 'gbk')
        .replace(/_appName_/g, name)
        .replace(/_displayName_/g, appName)
        .replace(/_version_/g, version)
        .replace(/_uninstallDesc_/g, uninstallDesc)
        .replace(/_outputPath_/g, outputPath)
        .replace(/_outputFileName_/g, outputFileName)
        .replace(/_filesPath_/g, files)
        .replace(/_setupResourcesPath_/g, setupResourcesPath)
        .replace(/_appPublisher_/g, appPublisher)
        .replace(/_appURL_/g, appURL)
        .replace(/_appId_/g, appId)
        .replace(/_platform_/g, '')

      fs.writeFile(tmpIssPath, iconv.encode(str, 'gbk'), null, function (err) {
        if (err) {
          reject(err)
          return
        }

        innosetupCompiler(tmpIssPath, { gui: false, verbose: true }, function (err) {
          fs.unlinkSync(tmpIssPath)

          if (err) {
            reject(err)
            return
          }

          resolve()
        })
      })
    })
  })
}

function packWinInstaller() {
  const version = packageInfo.version + '.0'

  return makeWinInstaller({
    name: 'StartGuildwars2',
    version,
    issPath: path.resolve(__dirname, 'Setup/setup.iss'),
    files: path.resolve(__dirname, 'bin/x64/Release'),
    outputPath: path.resolve(__dirname, 'Dist'),
    outputFileName: `StartGuildwars2-Setup-${version}`,
    setupResourcesPath: path.resolve(__dirname, 'Setup'),
    appPublisher: 'KeenGhost',
    appName: 'StartGuildwars2',
    appURL: 'https://gw2.keenghost.com',
    appId: 'BBCA18B7-0D0F-4BCC-892C-4A224DC9BA81',
    uninstallDesc: '卸载 StartGuildwars2',
  })
}

function updateVersion() {
  const version = packageInfo.version + '.0'
  let assemblyContent = fs.readFileSync(path.resolve(__dirname, 'Properties/AssemblyInfo.cs'), { encoding: 'utf8' })

  assemblyContent = assemblyContent.replace(/AssemblyVersion\(".*"\)/, `AssemblyVersion("${version}")`)
  assemblyContent = assemblyContent.replace(/AssemblyFileVersion\(".*"\)/, `AssemblyFileVersion("${version}")`)

  fs.writeFileSync(path.resolve(__dirname, 'Properties/AssemblyInfo.cs'), assemblyContent)

  console.log(`updated to v${version}`)

  return Promise.resolve()
}

gulp.task('build', gulp.series(
  packWinInstaller,
))

gulp.task('update', gulp.series(
  updateVersion,
))
