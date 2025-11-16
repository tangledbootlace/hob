import { getCustomers, getCustomer, createCustomer, updateCustomer, deleteCustomer } from '../customers';
import { apiClient } from '../client';

jest.mock('../client', () => ({
  apiClient: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    delete: jest.fn(),
  },
}));

describe('Customer API', () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('getCustomers', () => {
    it('should fetch customers with default pagination', async () => {
      const mockResponse = {
        items: [],
        page: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      };
      (apiClient.get as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await getCustomers();

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/customers?page=1&pageSize=20',
        { cache: 'no-store' }
      );
      expect(result).toEqual(mockResponse);
    });

    it('should fetch customers with custom pagination', async () => {
      const mockResponse = {
        items: [],
        page: 2,
        pageSize: 50,
        totalCount: 100,
        totalPages: 2,
      };
      (apiClient.get as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await getCustomers(2, 50);

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/customers?page=2&pageSize=50',
        { cache: 'no-store' }
      );
      expect(result).toEqual(mockResponse);
    });

    it('should fetch customers with search parameter', async () => {
      const mockResponse = {
        items: [],
        page: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      };
      (apiClient.get as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await getCustomers(1, 20, 'john');

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/customers?page=1&pageSize=20&search=john',
        { cache: 'no-store' }
      );
      expect(result).toEqual(mockResponse);
    });

    it('should not include search parameter when not provided', async () => {
      const mockResponse = {
        items: [],
        page: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      };
      (apiClient.get as jest.Mock).mockResolvedValueOnce(mockResponse);

      await getCustomers(1, 20);

      expect(apiClient.get).toHaveBeenCalledWith(
        '/api/customers?page=1&pageSize=20',
        { cache: 'no-store' }
      );
    });
  });

  describe('getCustomer', () => {
    it('should fetch a single customer by ID', async () => {
      const customerId = '123e4567-e89b-12d3-a456-426614174000';
      const mockCustomer = {
        customerId,
        name: 'John Doe',
        email: 'john@example.com',
        phone: '555-1234',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        orders: [],
      };
      (apiClient.get as jest.Mock).mockResolvedValueOnce(mockCustomer);

      const result = await getCustomer(customerId);

      expect(apiClient.get).toHaveBeenCalledWith(
        `/api/customers/${customerId}`,
        { cache: 'no-store' }
      );
      expect(result).toEqual(mockCustomer);
    });
  });

  describe('createCustomer', () => {
    it('should create a new customer', async () => {
      const newCustomer = {
        name: 'Jane Smith',
        email: 'jane@example.com',
        phone: '555-5678',
      };
      const mockResponse = {
        customerId: '123e4567-e89b-12d3-a456-426614174001',
        ...newCustomer,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        orders: [],
      };
      (apiClient.post as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await createCustomer(newCustomer);

      expect(apiClient.post).toHaveBeenCalledWith('/api/customers', newCustomer);
      expect(result).toEqual(mockResponse);
    });

    it('should create a customer without phone', async () => {
      const newCustomer = {
        name: 'No Phone User',
        email: 'noPhone@example.com',
      };
      const mockResponse = {
        customerId: '123e4567-e89b-12d3-a456-426614174002',
        ...newCustomer,
        phone: null,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        orders: [],
      };
      (apiClient.post as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await createCustomer(newCustomer);

      expect(apiClient.post).toHaveBeenCalledWith('/api/customers', newCustomer);
      expect(result).toEqual(mockResponse);
    });
  });

  describe('updateCustomer', () => {
    it('should update an existing customer', async () => {
      const customerId = '123e4567-e89b-12d3-a456-426614174000';
      const updateData = {
        name: 'Updated Name',
        email: 'updated@example.com',
        phone: '555-9999',
      };
      const mockResponse = {
        customerId,
        ...updateData,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        orders: [],
      };
      (apiClient.put as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await updateCustomer(customerId, updateData);

      expect(apiClient.put).toHaveBeenCalledWith(
        `/api/customers/${customerId}`,
        updateData
      );
      expect(result).toEqual(mockResponse);
    });

    it('should update customer with partial data', async () => {
      const customerId = '123e4567-e89b-12d3-a456-426614174000';
      const updateData = {
        name: 'Only Name Updated',
      };
      const mockResponse = {
        customerId,
        name: 'Only Name Updated',
        email: 'original@example.com',
        phone: '555-0000',
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
        orders: [],
      };
      (apiClient.put as jest.Mock).mockResolvedValueOnce(mockResponse);

      const result = await updateCustomer(customerId, updateData);

      expect(apiClient.put).toHaveBeenCalledWith(
        `/api/customers/${customerId}`,
        updateData
      );
      expect(result).toEqual(mockResponse);
    });
  });

  describe('deleteCustomer', () => {
    it('should delete a customer by ID', async () => {
      const customerId = '123e4567-e89b-12d3-a456-426614174000';
      (apiClient.delete as jest.Mock).mockResolvedValueOnce(undefined);

      await deleteCustomer(customerId);

      expect(apiClient.delete).toHaveBeenCalledWith(`/api/customers/${customerId}`);
    });

    it('should not return any value', async () => {
      const customerId = '123e4567-e89b-12d3-a456-426614174000';
      (apiClient.delete as jest.Mock).mockResolvedValueOnce(undefined);

      const result = await deleteCustomer(customerId);

      expect(result).toBeUndefined();
    });
  });
});
