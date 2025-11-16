import { ApiClient } from '../client';

describe('ApiClient', () => {
  let client: ApiClient;

  beforeEach(() => {
    client = new ApiClient('http://test-api.com');
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.resetAllMocks();
  });

  describe('constructor', () => {
    it('should create client with provided baseUrl', async () => {
      const customClient = new ApiClient('http://custom.com');
      const mockResponse = { data: 'test' };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await customClient.get('/test');

      expect(global.fetch).toHaveBeenCalledWith(
        'http://custom.com/test',
        expect.any(Object)
      );
    });

    it('should create client with default baseUrl from environment', async () => {
      const defaultClient = new ApiClient();
      const mockResponse = { data: 'test' };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await defaultClient.get('/test');

      expect(global.fetch).toHaveBeenCalledWith(
        'http://localhost:3000/api/test',
        expect.any(Object)
      );
    });
  });

  describe('get', () => {
    it('should make GET request with correct URL', async () => {
      const mockResponse = { data: 'test' };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await client.get('/test-endpoint');

      expect(global.fetch).toHaveBeenCalledWith(
        'http://test-api.com/test-endpoint',
        expect.objectContaining({
          method: 'GET',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
        })
      );
    });

    it('should return parsed JSON response', async () => {
      const mockResponse = { data: 'test', id: 123 };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const result = await client.get('/test-endpoint');

      expect(result).toEqual(mockResponse);
    });

    it('should throw error on failed request', async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        json: async () => ({ error: 'Resource not found' }),
      });

      await expect(client.get('/test-endpoint')).rejects.toThrow('Resource not found');
    });

    it('should throw error with status code when no error message', async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error',
        json: async () => { throw new Error('Invalid JSON'); },
      });

      await expect(client.get('/test-endpoint')).rejects.toThrow('API request failed: 500');
    });
  });

  describe('post', () => {
    it('should make POST request with correct URL and body', async () => {
      const mockData = { name: 'test' };
      const mockResponse = { id: 1, ...mockData };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await client.post('/test-endpoint', mockData);

      expect(global.fetch).toHaveBeenCalledWith(
        'http://test-api.com/test-endpoint',
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
          body: JSON.stringify(mockData),
        })
      );
    });

    it('should return parsed JSON response', async () => {
      const mockResponse = { id: 1, name: 'test' };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const result = await client.post('/test-endpoint', { name: 'test' });

      expect(result).toEqual(mockResponse);
    });

    it('should handle undefined body', async () => {
      const mockResponse = { success: true };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await client.post('/test-endpoint');

      expect(global.fetch).toHaveBeenCalledWith(
        'http://test-api.com/test-endpoint',
        expect.objectContaining({
          method: 'POST',
          body: undefined,
        })
      );
    });
  });

  describe('put', () => {
    it('should make PUT request with correct URL and body', async () => {
      const mockData = { name: 'updated' };
      const mockResponse = { id: 1, ...mockData };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await client.put('/test-endpoint/1', mockData);

      expect(global.fetch).toHaveBeenCalledWith(
        'http://test-api.com/test-endpoint/1',
        expect.objectContaining({
          method: 'PUT',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
          body: JSON.stringify(mockData),
        })
      );
    });

    it('should return parsed JSON response', async () => {
      const mockResponse = { id: 1, name: 'updated' };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const result = await client.put('/test-endpoint/1', { name: 'updated' });

      expect(result).toEqual(mockResponse);
    });
  });

  describe('delete', () => {
    it('should make DELETE request with correct URL', async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => ({}),
      });

      await client.delete('/test-endpoint/1');

      expect(global.fetch).toHaveBeenCalledWith(
        'http://test-api.com/test-endpoint/1',
        expect.objectContaining({
          method: 'DELETE',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
        })
      );
    });

    it('should not return any value', async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => ({}),
      });

      const result = await client.delete('/test-endpoint/1');

      expect(result).toBeUndefined();
    });

    it('should throw error on failed request', async () => {
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        status: 404,
        statusText: 'Not Found',
        json: async () => ({ error: 'Resource not found' }),
      });

      await expect(client.delete('/test-endpoint/1')).rejects.toThrow('Resource not found');
    });
  });

  describe('custom headers', () => {
    it('should merge custom headers with default headers', async () => {
      const mockResponse = { data: 'test' };
      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      await client.get('/test-endpoint', {
        headers: { 'X-Custom-Header': 'custom-value' },
      });

      expect(global.fetch).toHaveBeenCalledWith(
        'http://test-api.com/test-endpoint',
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            'X-Custom-Header': 'custom-value',
          }),
        })
      );
    });
  });
});
