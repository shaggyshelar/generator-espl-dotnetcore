'use strict';
const Generator = require('yeoman-generator');
const chalk = require('chalk');
const yosay = require('yosay');

module.exports = class extends Generator {
  prompting() {
    // Have Yeoman greet the user.
    this.log(yosay(
      'Welcome to the priceless ' + chalk.red('generator-espl-dotnetcore') + ' generator!'
    ));

    if (this.options.name === undefined) {
      const prompts = [{
        type: String,
        name: 'name',
        message: 'What is the name of this entity?',
        required: true
      },
      {
        type: String,
        name: 'directory',
        message: 'Sub-directory/namespace this entity lives in? Leave empty for root entities.',
        default: ''
      },
      {
        type: 'confirm',
        name: 'createController',
        message: 'Should we create controller for this entity?',
        default: true
      },
      {
        type: 'checkbox',
        name: 'entityOptions',
        message: 'Please select what you want to generate for this entity.',
        choices: [
          { name: 'Add Service Layer?', checked: true, value: 'enableIdentity'  },
          { name: 'Is it Auditable Entity?', checked: true, value: 'enableCORS'  },
          { name: 'Enable Node Service?', checked: true, value: 'enableNodeService'  }
        ]
      }];
      return this.prompt(prompts).then(props => {
        props.namespace = this._getNamespace(props.directory);
        // To access props later use this.props.someAnswer;
        this.props = props;
      });
    }
    // Set the args from the options
    this.args = {
      'name': this.options.name,
      'directory': this.options.directory,
      'createController': this.options.createController,
      'namespace': this._getNamespace(this.options.directory)
    };
  }

  _getNamespace(directory) {
    return (directory == '')
      ? 'Models.Domain'
      : 'Models.Domain.' + directory;
  }

  writing() {
    var srcDir = (this.config.get('srcDirectory') == '') ? '' : this.config.get('srcDirectory') + '/';
    var filename = this.args.name + '.cs';
    var filepath = (this.args.directory == '')
      ? srcDir + 'Models.Domain/' + filename
      : srcDir + 'Models.Domain/' + this.args.directory + '/' + filename;

    // this.log('srcDir' + srcDir);
    // this.log('filename' + filename);
    // this.log('filepath' + filepath);
  }

  end() {
    this.log('NOTE: Make sure you add the new model as a DbSet<> in your ApplicationDbContext and it\'s property mappings.');
  }
};
