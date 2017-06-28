'use strict';
const Generator = require('yeoman-generator');
const chalk = require('chalk');
const yosay = require('yosay');
var shouldContinue = true;

module.exports = class extends Generator {
  prompting() {
    if (this.fs.exists('.yo-rc.json') || this.fs.exists('WebApi/WebApi.xproj')) {
      shouldContinue = false;
      this.log(chalk.red('The directory must be empty. Stopping.'));
      return;
    }

    // Have Yeoman greet the user.
    this.log(yosay(
      'Welcome to the slick ' + chalk.red('espl-dotnetcore') + ' generator!'
    ));

    var prompts = [
      {
        type: String,
        name: 'appName',
        message: 'Name this application. [a-z,A-Z] No spaces or special characters.',
        default: 'WebApi'
      },
      {
        type: 'confirm',
        name: 'useSrcDirectory',
        message: 'Would you like the solution files to be in a "src" directory?',
        default: false
      }];

    return this.prompt(prompts).then(function (args) {
      var srcDirectory = (args.useSrcDirectory) ? 'src' : '';

      this.config.set('appName', args.appName);
      this.config.set('srcDirectory', srcDirectory);

      args.srcDirectory = (srcDirectory == '') ? '' : srcDirectory + '\\';

      this.args = args;
    }.bind(this));
  }

  writing() {
    if (!shouldContinue) {
      return;
    }

    this.fs.copyTpl(
      this.templatePath('genericSolution.sln'),
      this.destinationPath(this.args.appName + '.sln'),
      { srcDirectory: '' }
    );

    // Copy everything else in
    this.fs.copy(
      this.templatePath('toCopy/**/*'),
      this.destinationPath(this.args.srcDirectory)
    );
  }

  install() {
    if (!shouldContinue) {
      return;
    }
    // this.installDependencies();
  }

  end() {
    this.log(chalk.green('  ALMOST THERE! ') +
      'There are a few more things you\'ll have to do before you can run the new solution. ' +
      'Check out the readme.md file in the root of the solution folder for further instructions.');
  }
};
