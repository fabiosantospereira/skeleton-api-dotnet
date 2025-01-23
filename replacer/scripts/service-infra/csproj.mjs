import { copyProjectFile } from '../../utils/file-operations.mjs';

export default copyProjectFile;

export const file = 'src/services/academic/Looplex.DotNet.Samples.Academic.Infra/Looplex.DotNet.Samples.Academic.Infra.csproj';
export const outputFile = '{{PROJECT_PATH}}/services/{{MODULE_NAME_CC}}/{{PROJECT_NAMESPACE}}.{{MODULE_NAME}}.Infra/{{PROJECT_NAMESPACE}}.{{MODULE_NAME}}.Infra.csproj';