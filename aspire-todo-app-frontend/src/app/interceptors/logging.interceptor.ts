import {
  HttpEvent,
  HttpHandlerFn,
  HttpRequest,
  HttpResponse,
  HttpErrorResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { Observable, tap } from 'rxjs';

export function loggingInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
): Observable<HttpEvent<unknown>> {
  const toastr = inject(ToastrService);
  const started = Date.now();

  return next(req).pipe(
    tap({
      next: (event) => {
        if (
          event instanceof HttpResponse &&
          req.method !== 'GET' &&
          !req.urlWithParams.includes('graphql') &&
          !req.urlWithParams.includes('Database')
        ) {
          const elapsed = Date.now() - started;
          toastr.info(
            `<code class="bg-primary-hover p-0.5 px-2 rounded-xl text-pretty">${req.urlWithParams}</code> completed in ${elapsed} ms`,
            'REST request',
            {
              timeOut: 3000,
              tapToDismiss: false,
              positionClass: 'toast-bottom-left',
              toastClass: 'ngx-toastr my-toast-info toast-shadow-remove',
              enableHtml: true,
              newestOnTop: false,
            }
          );
        }

        if (
          event instanceof HttpResponse &&
          req.urlWithParams.includes('graphql') &&
          !(req.body as { query: string }).query.includes('GetTasks')
        ) {
          const graphQlQuery = (req.body as { query: string }).query.trim();
          const elapsed = Date.now() - started;

          toastr.info(
            `<pre class="bg-primary-hover p-2 rounded-xl text-white whitespace-pre-wrap text-sm font-mono leading-snug max-w-[600px] overflow-auto">${graphQlQuery}</pre> completed in ${elapsed} ms`,
            'GraphQL request',
            {
              timeOut: 3000,
              tapToDismiss: false,
              positionClass: 'toast-bottom-left',
              toastClass: 'ngx-toastr my-toast-info toast-shadow-remove',
              enableHtml: true,
              newestOnTop: false,
            }
          );
        }
      },
      error: (error: HttpErrorResponse) => {
        toastr.error(`${error.error.title}`, 'Error', {
          timeOut: 5000,
          positionClass: 'toast-bottom-left',
          toastClass: 'ngx-toastr toast-shadow-remove',
          newestOnTop: false,
        });
      },
    })
  );
}
