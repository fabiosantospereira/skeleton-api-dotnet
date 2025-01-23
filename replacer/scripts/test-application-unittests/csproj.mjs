import { CommandProcessor } from '../../utils/commandProcessor.mjs';
export default (filePath, outputFilePath) => 
  CommandProcessor.process({
    filePath,
    outputFilePath,
    patterns: replaces
  });

  const replaces = [
  {
    original: 'Looplex.DotNet.Samples',
    find: /Looplex.DotNet.Samples/g,
    replace: '{{PROJECT_NAMESPACE}}'
  },
  {
    original: 'Academic',
    find: /Academic/g,
    replace: '{{MODULE_NAME}}'
  },
  {
    original: 'academic',
    find: /academic/g,
    replace: '{{MODULE_NAME_CC}}'
  },
  {
    original: 'src',
    find: /src/g,
    replace: '{{PROJECT_PATH}}'
}
]

export const file = 'test/Looplex.DotNet.Samples.Academic.Application.UnitTests/Looplex.DotNet.Samples.Academic.Application.UnitTests.csproj';
export const outputFile = '{{TESTPROJECT_PATH}}/{{PROJECT_NAMESPACE}}.{{MODULE_NAME}}.Application.UnitTests/{{PROJECT_NAMESPACE}}.{{MODULE_NAME}}.Application.UnitTests.csproj';