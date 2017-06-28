'use strict';
var path = require('path');
var assert = require('yeoman-assert');
var helpers = require('yeoman-test');

describe('generator-espl-dotnetcore:controller', () => {
  beforeAll(() => {
    return helpers.run(path.join(__dirname, '../generators/controller'))
      .withPrompts({someAnswer: true});
  });

  it('creates files', () => {
    assert.file([
      'dummyfile.txt'
    ]);
  });
});
