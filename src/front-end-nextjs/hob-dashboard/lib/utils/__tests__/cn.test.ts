import { cn } from '../cn';

describe('cn utility', () => {
  it('should merge single class name', () => {
    const result = cn('test-class');
    expect(result).toBe('test-class');
  });

  it('should merge multiple class names', () => {
    const result = cn('class-1', 'class-2', 'class-3');
    expect(result).toContain('class-1');
    expect(result).toContain('class-2');
    expect(result).toContain('class-3');
  });

  it('should handle conditional class names', () => {
    const isActive = true;
    const result = cn('base-class', isActive && 'active-class');
    expect(result).toContain('base-class');
    expect(result).toContain('active-class');
  });

  it('should filter out false conditional class names', () => {
    const isActive = false;
    const result = cn('base-class', isActive && 'active-class');
    expect(result).toContain('base-class');
    expect(result).not.toContain('active-class');
  });

  it('should handle undefined and null values', () => {
    const result = cn('class-1', undefined, 'class-2', null, 'class-3');
    expect(result).toContain('class-1');
    expect(result).toContain('class-2');
    expect(result).toContain('class-3');
  });

  it('should merge Tailwind conflicting classes correctly', () => {
    // tailwind-merge should keep the last class when they conflict
    const result = cn('px-2', 'px-4');
    expect(result).toBe('px-4');
  });

  it('should handle array of class names', () => {
    const result = cn(['class-1', 'class-2'], 'class-3');
    expect(result).toContain('class-1');
    expect(result).toContain('class-2');
    expect(result).toContain('class-3');
  });

  it('should handle object notation for conditional classes', () => {
    const result = cn({
      'class-1': true,
      'class-2': false,
      'class-3': true,
    });
    expect(result).toContain('class-1');
    expect(result).not.toContain('class-2');
    expect(result).toContain('class-3');
  });

  it('should merge complex Tailwind classes', () => {
    const result = cn(
      'bg-red-500 text-white',
      'hover:bg-red-600',
      'dark:bg-red-700'
    );
    expect(result).toContain('bg-red-500');
    expect(result).toContain('text-white');
    expect(result).toContain('hover:bg-red-600');
    expect(result).toContain('dark:bg-red-700');
  });

  it('should deduplicate identical classes', () => {
    const result = cn('class-1', 'class-1', 'class-2');
    // Count occurrences - should only have one 'class-1'
    const matches = result.match(/class-1/g);
    expect(matches).toHaveLength(1);
  });

  it('should handle empty inputs', () => {
    const result = cn();
    expect(result).toBe('');
  });

  it('should handle mixed types of inputs', () => {
    const result = cn(
      'base-class',
      ['array-class-1', 'array-class-2'],
      { 'object-class': true },
      true && 'conditional-class'
    );
    expect(result).toContain('base-class');
    expect(result).toContain('array-class-1');
    expect(result).toContain('array-class-2');
    expect(result).toContain('object-class');
    expect(result).toContain('conditional-class');
  });
});
