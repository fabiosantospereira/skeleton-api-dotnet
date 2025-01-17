import fs from 'fs';

export default function (filePath, outputFilePath) {
  // Read the file content
  let content = fs.readFileSync(filePath, 'utf8');

  const replaces = [
    {
      original: 'src',
      find: /src/g,
      replace: '{{WEB_API_PROJECT_PATH}}'
    },
    {
      original: 'Looplex.DotNet.Samples.WebApi',
      find: /Looplex.DotNet.Samples.WebApi/g,
      replace: '{{WEB_API_PROJECT_NAME}}'
    }
  ]

  replaces.forEach(replace => {
    content = content
      .replace(replace.find, replace.replace);
  });
  fs.writeFileSync(outputFilePath, content, 'utf8');
  console.log(`Processed file saved to: ${outputFilePath}`);

  return replaces;
}

export const file = 'src/Looplex.DotNet.Samples.WebApi/Dockerfile';
export const outputFile = '{{WEB_API_PROJECT_NAME}}/Dockerfile';