type ApiErrorPayload = {
  message?: string;
  detail?: string;
  error?: string;
  errors?: Array<{ detail?: string; errorMessage?: string; message?: string }>;
};

export function parseApiError(error: unknown, fallback: string): string {
  if (!error || typeof error !== 'object') {
    return fallback;
  }

  const httpError = error as { status?: number; error?: unknown; message?: string };

  if (httpError.status === 0) {
    return 'Unable to reach the API. Check that the backend is running.';
  }

  const payload = httpError.error;

  if (Array.isArray(payload)) {
    const messages = payload
      .map((entry) => {
        if (typeof entry === 'string') {
          return entry;
        }

        if (entry && typeof entry === 'object') {
          const item = entry as { errorMessage?: string; message?: string };
          return item.errorMessage ?? item.message;
        }

        return undefined;
      })
      .filter((message): message is string => !!message);

    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  if (payload && typeof payload === 'object') {
    const body = payload as ApiErrorPayload;

    if (body.errors?.length) {
      const messages = body.errors
        .map((entry) => entry.detail ?? entry.errorMessage ?? entry.message)
        .filter((message): message is string => !!message);

      if (messages.length > 0) {
        return messages.join(' ');
      }
    }

    if (body.detail) {
      return body.detail;
    }

    if (body.message) {
      return body.message;
    }

    if (body.error) {
      return body.error;
    }
  }

  if (typeof payload === 'string' && payload.trim().length > 0) {
    return payload;
  }

  if (httpError.message) {
    return httpError.message;
  }

  return fallback;
}
