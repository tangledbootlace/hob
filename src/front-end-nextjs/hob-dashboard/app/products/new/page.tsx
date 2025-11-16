import Link from "next/link";
import { ArrowLeft } from "lucide-react";
import { createProductAction } from "@/actions/products";
import { ProductForm } from "@/components/products/ProductForm";

export default function NewProductPage() {
  return (
    <div className="container mx-auto max-w-3xl px-4 py-8">
      <div className="mb-6">
        <Link
          href="/products"
          className="inline-flex items-center text-sm text-blue-600 hover:text-blue-800"
        >
          <ArrowLeft className="mr-1 h-4 w-4" />
          Back to Products
        </Link>
      </div>

      <h1 className="mb-6 text-3xl font-bold">Add New Product</h1>

      <ProductForm action={createProductAction} />
    </div>
  );
}
