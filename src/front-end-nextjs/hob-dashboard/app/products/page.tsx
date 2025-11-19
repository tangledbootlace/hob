import Link from "next/link";
import { getProducts } from "@/lib/api/products";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Package, Plus, AlertTriangle } from "lucide-react";

interface SearchParams {
  page?: string;
  pageSize?: string;
  search?: string;
  category?: string;
  lowStock?: string;
}

interface PageProps {
  searchParams: Promise<SearchParams>;
}

export default async function ProductsPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const page = parseInt(params.page || "1");
  const pageSize = parseInt(params.pageSize || "20");
  const search = params.search;
  const category = params.category;
  const lowStock = params.lowStock === "true" ? true : undefined;

  const { items: products, totalPages } = await getProducts(
    page,
    pageSize,
    search,
    category,
    lowStock
  );

  return (
    <div className="container mx-auto max-w-7xl px-4 py-8">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Products</h1>
          <p className="mt-2 text-[var(--muted-foreground)]">Manage your product catalog and inventory</p>
        </div>
        <Link href="/products/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" />
            Add Product
          </Button>
        </Link>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>
            <div className="flex items-center gap-2">
              <Package className="h-5 w-5" />
              Product List
            </div>
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>SKU</TableHead>
                  <TableHead>Name</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead className="text-right">Unit Price</TableHead>
                  <TableHead className="text-right">Stock</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {products.map((product) => (
                  <TableRow key={product.productId}>
                    <TableCell className="font-mono text-sm">{product.sku}</TableCell>
                    <TableCell className="font-medium">
                      <div className="flex items-center gap-2">
                        {product.name}
                        {product.isLowStock && (
                          <AlertTriangle className="h-4 w-4 text-yellow-500" aria-label="Low Stock" />
                        )}
                      </div>
                    </TableCell>
                    <TableCell>{product.category || "-"}</TableCell>
                    <TableCell className="text-right">
                      ${product.unitPrice.toFixed(2)}
                    </TableCell>
                    <TableCell className="text-right">
                      <span className={product.isLowStock ? "text-yellow-600 dark:text-yellow-500 font-semibold" : ""}>
                        {product.stockQuantity}
                      </span>
                      <span className="text-[var(--muted-foreground)] text-sm"> / {product.lowStockThreshold}</span>
                    </TableCell>
                    <TableCell>
                      <span
                        className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                          product.isActive
                            ? "bg-green-100 text-green-700"
                            : "bg-gray-100 text-gray-700"
                        }`}
                      >
                        {product.isActive ? "Active" : "Inactive"}
                      </span>
                    </TableCell>
                    <TableCell>
                      <Link href={`/products/${product.productId}`}>
                        <Button variant="outline" size="sm">
                          View
                        </Button>
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
                {products.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center text-[var(--muted-foreground)] py-8">
                      No products found
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </div>

          {totalPages > 1 && (
            <div className="mt-4 flex justify-center gap-2">
              {page > 1 && (
                <Link href={`/products?page=${page - 1}`}>
                  <Button variant="outline">Previous</Button>
                </Link>
              )}
              <span className="flex items-center px-4 text-sm text-[var(--muted-foreground)]">
                Page {page} of {totalPages}
              </span>
              {page < totalPages && (
                <Link href={`/products?page=${page + 1}`}>
                  <Button variant="outline">Next</Button>
                </Link>
              )}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
