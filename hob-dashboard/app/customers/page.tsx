import Link from "next/link";
import { getCustomers } from "@/lib/api/customers";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Plus } from "lucide-react";

export const dynamic = "force-dynamic";

interface PageProps {
  searchParams: Promise<{ page?: string; search?: string }>;
}

export default async function CustomersPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const page = Number(params.page) || 1;
  const search = params.search || undefined;

  const data = await getCustomers(page, 20, search);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Customers</h1>
          <p className="text-gray-600 mt-2">Manage your customer database</p>
        </div>
        <Link href="/customers/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Customer
          </Button>
        </Link>
      </div>

      <div className="bg-white rounded-lg border border-gray-200">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Phone</TableHead>
              <TableHead>Orders</TableHead>
              <TableHead>Created</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center py-8 text-gray-500">
                  No customers found
                </TableCell>
              </TableRow>
            ) : (
              data.items.map((customer) => (
                <TableRow key={customer.customerId}>
                  <TableCell className="font-medium">
                    <Link href={`/customers/${customer.customerId}`} className="hover:text-blue-600 hover:underline">
                      {customer.name}
                    </Link>
                  </TableCell>
                  <TableCell>{customer.email}</TableCell>
                  <TableCell>{customer.phone || "-"}</TableCell>
                  <TableCell>{customer.orders?.length || 0}</TableCell>
                  <TableCell>{new Date(customer.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell className="text-right">
                    <Link href={`/customers/${customer.customerId}`}>
                      <Button variant="outline" size="sm">View</Button>
                    </Link>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {data.totalPages > 1 && (
        <div className="flex justify-center gap-2">
          {page > 1 && (
            <Link href={`/customers?page=${page - 1}${search ? `&search=${search}` : ""}`}>
              <Button variant="outline">Previous</Button>
            </Link>
          )}
          <span className="flex items-center px-4 text-sm text-gray-600">
            Page {page} of {data.totalPages}
          </span>
          {page < data.totalPages && (
            <Link href={`/customers?page=${page + 1}${search ? `&search=${search}` : ""}`}>
              <Button variant="outline">Next</Button>
            </Link>
          )}
        </div>
      )}
    </div>
  );
}
