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
        type: 'checkbox',
        name: 'projectOptions',
        message: 'Please select what you want to include in this project.',
        choices: [
          { name: 'Enable Microsoft Identity?', checked: true, value: 'enableIdentity'  },
          { name: 'Enable CORS?', checked: true, value: 'enableCORS'  },
          { name: 'Enable Node Service?', checked: true, value: 'enableNodeService'  },
          { name: 'Enable Rate Limiting Rules?', checked: true, value: 'enableRateLimiting'  },
          { name: 'Enable Static Files?', checked: true, value: 'enableStaticFiles'  },
          { name: 'Enable PDF Export?', checked: true, value: 'enablePDFExport'  },
          { name: 'Enable Excel Export?', checked: true, value: 'enableExcelExport'  },
          { name: 'Enbable Memory Cache?', checked: true, value: 'enableMomoryCache'  },
          { name: 'Enbable Localization?', checked: true, value: 'enableLocalization'  },
          { name: 'Enbable Sample Data?', checked: false, value: 'enableSampleData'  }
        ]
      },
      {
        type: String,
        name: 'tokenIssuer',
        message: 'Please enter token issuer',
        default: 'http://example.com',
        require: true
      },
      {
        type: String,
        name: 'tokenAudience',
        message: 'Please enter token audience',
        default: 'http://example.com',
        require: true
      },
      {
        type: String,
        name: 'connectionStrings',
        message: 'Please enter connection strings',
        default: 'Server=.\\SQLExpress;Database=dbName;Trusted_Connection=True;',
        require: true
      }];

    return this.prompt(prompts).then(function (args) {
      var srcDirectory = '';

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

    // // Copy everything else in
    // this.fs.copy(
    //   this.templatePath('toCopy/**/*'),
    //   this.destinationPath(this.args.srcDirectory)
    // );
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
