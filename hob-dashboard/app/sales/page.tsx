import Link from "next/link";
import { getSales } from "@/lib/api/sales";
import { Button } from "@/components/ui/button";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Plus } from "lucide-react";

export const dynamic = "force-dynamic";

interface PageProps {
  searchParams: Promise<{ page?: string; orderId?: string; search?: string }>;
}

export default async function SalesPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const page = Number(params.page) || 1;
  const orderId = params.orderId || undefined;
  const search = params.search || undefined;

  const data = await getSales(page, 20, orderId, search);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Sales</h1>
          <p className="text-gray-600 mt-2">Manage individual sale items and products</p>
        </div>
        <Link href="/sales/new">
          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Sale
          </Button>
        </Link>
      </div>

      <div className="bg-white rounded-lg border border-gray-200">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Product Name</TableHead>
              <TableHead>Quantity</TableHead>
              <TableHead>Unit Price</TableHead>
              <TableHead>Total Price</TableHead>
              <TableHead>Created</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center py-8 text-gray-500">
                  No sales found
                </TableCell>
              </TableRow>
            ) : (
              data.items.map((sale) => (
                <TableRow key={sale.saleId}>
                  <TableCell className="font-medium">
                    <Link href={`/sales/${sale.saleId}`} className="hover:text-blue-600 hover:underline">
                      {sale.productName}
                    </Link>
                  </TableCell>
                  <TableCell>{sale.quantity}</TableCell>
                  <TableCell>${sale.unitPrice.toFixed(2)}</TableCell>
                  <TableCell>${sale.totalPrice.toFixed(2)}</TableCell>
                  <TableCell>{new Date(sale.createdAt).toLocaleDateString()}</TableCell>
                  <TableCell className="text-right">
                    <Link href={`/sales/${sale.saleId}`}>
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
            <Link href={`/sales?page=${page - 1}`}>
              <Button variant="outline">Previous</Button>
            </Link>
          )}
          <span className="flex items-center px-4 text-sm text-gray-600">
            Page {page} of {data.totalPages}
          </span>
          {page < data.totalPages && (
            <Link href={`/sales?page=${page + 1}`}>
              <Button variant="outline">Next</Button>
            </Link>
          )}
        </div>
      )}
    </div>
  );
}
