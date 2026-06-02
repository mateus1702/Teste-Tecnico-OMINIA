import { parseApiError } from './api-error.util';

describe('parseApiError', () => {
  it('returns middleware detail when present', () => {
    const message = parseApiError(
      {
        error: {
          detail: 'Cannot sell more than 20 identical items.',
          message: 'Business rule violation',
        },
      },
      'fallback',
    );

    expect(message).toBe('Cannot sell more than 20 identical items.');
  });

  it('returns fluent validation array messages', () => {
    const message = parseApiError(
      {
        error: [{ errorMessage: 'Customer name is required' }],
      },
      'fallback',
    );

    expect(message).toBe('Customer name is required');
  });

  it('returns fallback for unknown errors', () => {
    expect(parseApiError({}, 'Failed to create sale')).toBe('Failed to create sale');
  });
});
