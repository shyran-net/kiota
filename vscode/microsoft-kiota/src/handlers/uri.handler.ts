import * as vscode from "vscode";
import { openTreeViewWithProgress } from "../utilities/file";
import { OpenApiTreeProvider } from "../openApiTreeProvider";

export class UriHandler {
  private _openApiTreeProvider: OpenApiTreeProvider;

  constructor(private openApiTreeProvider: OpenApiTreeProvider) {
    this._openApiTreeProvider = openApiTreeProvider;
  }

  async handleUri(uri: vscode.Uri) {
    if (uri.path === "/") {
      return;
    }
    const queryParameters = this.getQueryParameters(uri);
    if (uri.path.toLowerCase() === "/opendescription") {

      // TODO: uncomment when telemetry is implemented
      // reporter.sendTelemetryEvent("DeepLink.OpenDescription");
      const descriptionUrl = queryParameters["descriptionurl"];
      if (descriptionUrl) {
        await openTreeViewWithProgress(() => this._openApiTreeProvider.setDescriptionUrl(descriptionUrl));
        return;
      }
    }
    void vscode.window.showErrorMessage(
      vscode.l10n.t("Invalid URL, please check the documentation for the supported URLs")
    );

  };

  getQueryParameters(uri: vscode.Uri): Record<string, string> {
    const query = uri.query;
    if (!query) {
      return {};
    }
    const queryParameters = (query.startsWith('?') ? query.substring(1) : query).split("&");
    const parameters = {} as Record<string, string>;
    queryParameters.forEach((element) => {
      const keyValue = element.split("=");
      parameters[keyValue[0].toLowerCase()] = decodeURIComponent(keyValue[1]);
    });
    return parameters;
  }

}





