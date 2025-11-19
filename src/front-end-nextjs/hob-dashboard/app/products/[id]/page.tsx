import Link from "next/link";
import { ArrowLeft, Pencil, AlertTriangle } from "lucide-react";
import { getProduct } from "@/lib/api/products";
import { DeleteProductButton } from "@/components/products/delete-product-button";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface PageProps {
  params: Promise<{ id: string }>;
}

export default async function ProductDetailPage({ params }: PageProps) {
  const { id } = await params;
  const product = await getProduct(id);

  return (
    <div className="container mx-auto max-w-4xl px-4 py-8">
      <div className="mb-6">
        <Link
          href="/products"
          className="inline-flex items-center text-sm text-[var(--primary)] hover:underline"
        >
          <ArrowLeft className="mr-1 h-4 w-4" />
          Back to Products
        </Link>
      </div>

      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">{product.name}</h1>
          <p className="mt-1 text-sm text-[var(--muted-foreground)]">SKU: {product.sku}</p>
        </div>
        <div className="flex gap-2">
          <Link href={`/products/${product.productId}/edit`}>
            <Button variant="outline">
              <Pencil className="mr-2 h-4 w-4" />
              Edit
            </Button>
          </Link>
          <DeleteProductButton
            productId={product.productId}
            hasSales={(product.salesCount ?? 0) > 0}
            salesCount={product.salesCount ?? 0}
          />
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Product Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm font-medium text-[var(--muted-foreground)]">Description</p>
              <p className="mt-1">{product.description || "-"}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-[var(--muted-foreground)]">Category</p>
              <p className="mt-1">{product.category || "-"}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-[var(--muted-foreground)]">Status</p>
              <p className="mt-1">
                <span
                  className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                    product.isActive
                      ? "bg-green-100 text-green-700"
                      : "bg-gray-100 text-gray-700"
                  }`}
                >
                  {product.isActive ? "Active" : "Inactive"}
                </span>
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Pricing & Inventory</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm font-medium text-[var(--muted-foreground)]">Unit Price</p>
              <p className="mt-1 text-2xl font-bold">${product.unitPrice.toFixed(2)}</p>
            </div>

            <div>
              <p className="text-sm font-medium text-[var(--muted-foreground)]">Stock Quantity</p>
              <p className={`mt-1 text-2xl font-bold ${product.isLowStock ? "text-yellow-600 dark:text-yellow-500" : ""}`}>
                {product.stockQuantity}
                {product.isLowStock && (
                  <AlertTriangle className="ml-2 inline h-5 w-5" aria-label="Low Stock Alert" />
                )}
              </p>
            </div>

            <div>
              <p className="text-sm font-medium text-[var(--muted-foreground)]">Low Stock Threshold</p>
              <p className="mt-1">{product.lowStockThreshold}</p>
            </div>

            {product.isLowStock && (
              <div className="rounded-md bg-yellow-50 dark:bg-yellow-950/20 border border-yellow-200 dark:border-yellow-900 p-4">
                <div className="flex">
                  <AlertTriangle className="h-5 w-5 text-yellow-400" />
                  <div className="ml-3">
                    <h3 className="text-sm font-medium text-yellow-800 dark:text-yellow-500">
                      Low Stock Warning
                    </h3>
                    <div className="mt-2 text-sm text-yellow-700 dark:text-yellow-400">
                      <p>
                        Current stock ({product.stockQuantity}) is at or below the low stock threshold ({product.lowStockThreshold}).
                        Consider reordering soon.
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Metadata</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-2">
          <div>
            <p className="text-sm font-medium text-[var(--muted-foreground)]">Created At</p>
            <p className="mt-1">{new Date(product.createdAt).toLocaleString()}</p>
          </div>

          <div>
            <p className="text-sm font-medium text-[var(--muted-foreground)]">Last Updated</p>
            <p className="mt-1">{new Date(product.updatedAt).toLocaleString()}</p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
