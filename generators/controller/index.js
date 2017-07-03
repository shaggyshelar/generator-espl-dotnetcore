'use strict';
const Generator = require('yeoman-generator');
const chalk = require('chalk');
const yosay = require('yosay');

module.exports = class extends Generator {
  prompting() {
    // Have Yeoman greet the user.
    this.log(yosay(
      'Welcome to the geometric ' + chalk.red('generator-espl-dotnetcore') + ' generator!'
    ));

    const prompts = [{
      type: 'list',
      name: 'list1',
      message: 'List 1 options',
      choices: [{name: 'jumbo'}, {name: 'tumbo', default: true}, {name: 'numbo'}]
    },
    {
      type: 'checkbox',
      name: 'list2',
      message: 'List 1 options',
      choices: [{name: 'jumbo'}, {name: 'tumbo', checked: true, default: true}, {name: 'numbo', checked: true}]
    }];

    return this.prompt(prompts).then(props => {
      // To access props later use this.props.someAnswer;
      this.props = props;
    });
  }

  writing() {
  }

  install() {
  }
};
