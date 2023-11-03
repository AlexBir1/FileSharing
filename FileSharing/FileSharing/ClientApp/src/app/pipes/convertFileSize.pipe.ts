import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
  name: 'useStandartFileSizes',
  pure: false,
})

export class StandartFileSizerPipe implements PipeTransform {
  transform(sourceSize: number): string {
      var result = '';
      var typeOfSize = ' Bytes';
      if (sourceSize > 1000) { // kb
        if (sourceSize > 1000000) { // mb
          if (sourceSize > 1000000000) { // gb
            result = (sourceSize / 1000000000).toFixed(2);
            typeOfSize = ' Gb';
            return String(result + typeOfSize);
          }
          result = (sourceSize / 1000000).toFixed(2);
          typeOfSize = ' Mb';
          return String(result + typeOfSize);
        }
        result = (sourceSize / 1000).toFixed(2);
        typeOfSize = ' Kb';
        return String(result + typeOfSize);
      }
      result = sourceSize.toString();
      return String(result + typeOfSize);
  }
}
